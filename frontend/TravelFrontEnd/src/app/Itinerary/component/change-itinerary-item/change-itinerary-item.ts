import { Component, ElementRef, Input, OnInit, ViewChild, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import {
  CdkDragDrop,
  moveItemInArray,
  transferArrayItem,
  CdkDropListGroup,
  CdkDropList,
  CdkDrag
} from '@angular/cdk/drag-drop';
import { DayPlan, ItineraryItem } from '../../interface/itinerarymainmodel';
import { ActivatedRoute, RouterModule } from '@angular/router';
declare const google: any;

@Component({
  selector: 'app-itinerary-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, CdkDropListGroup, CdkDropList, CdkDrag, RouterModule],
  templateUrl: './change-itinerary-item.html',
  styleUrl: './change-itinerary-item.css',
})
export class ItineraryDetailComponent implements OnInit {
  private http = inject(HttpClient);

  @Input() itineraryId!: number;
  @ViewChild('searchInput') searchInput!: ElementRef;

  showSearchModal = false;
  title = '';
  date = '';
  imageUrl = '';
  days: DayPlan[] = [];

  /** 目前顯示的天數（預設第1天） */
  activeDayIndex = 1;

  /** 所有行程總數（地圖圖例用） */
  get totalItems(): number {
    return this.days.reduce((sum, d) => sum + d.items.length, 0);
  }

  private currentAddingDay?: DayPlan;

  constructor(private activateroute: ActivatedRoute) { }

  ngOnInit() {
    this.itineraryId = Number(this.activateroute.snapshot.params['id']);
    if (this.itineraryId) {
      this.loadData();
    }
  }

  private mapApiToDays(items: ItineraryItem[]): DayPlan[] {
    if (!items || items.length === 0) return [];
    const groups = new Map<number, ItineraryItem[]>();
    items.forEach(item => {
      const d = item.dayNumber || 1;
      if (!groups.has(d)) groups.set(d, []);
      groups.get(d)?.push(item);
    });
    return Array.from(groups.entries())
      .sort(([a], [b]) => a - b)
      .map(([dayNumber, dayItems]) => ({
        day: dayNumber,
        items: dayItems.sort((a, b) => (a.sortOrder || 0) - (b.sortOrder || 0))
      }));
  }

  uploadImage(event: any) {
    const file = event.target.files[0];
    if (!file) return;
    const formDataimg = new FormData();
    formDataimg.append('image', file);
    this.http.post<any>(`https://localhost:7276/api/Itinerary/Savephoto/${this.itineraryId}`, formDataimg)
      .subscribe({
        next: (res) => { this.imageUrl = res.url; alert('封面更新成功！'); },
        error: (err) => { console.error('上傳失敗', err); alert('上傳失敗，請檢查網絡'); }
      });
  }
  parseAiDescription(item: any) {
    const desc = item.contentDescription || '';

    // 如果字串開頭是我們定義的標籤
    if (desc.startsWith('[AI_NEW_PLACE]')) {
      // 拆分字串: [標籤]|名稱|PlaceId|地址|緯度|經度|細節
      const parts = desc.split('|');

      // 更新 item 物件的顯示欄位
      item.attractionName = parts[1] || '未知地點';
      // 🛡️ 防禦：如果 AI 沒給 ID，我們給它一個特殊字串 "TEMP_AI_PLACE"
      item.placeId = (parts[2] && parts[2] !== '0') ? parts[2] : "TEMP_AI_PLACE";
      item.address = parts[3] || '請確認地址';
      item.latitude = parseFloat(parts[4]) || null;
      item.longitude = parseFloat(parts[5]) || null;
      item.contentDescription = parts[6] || ''; // 把後面的備註還給 description
      item.isAiSuggestion = true; // 標記為 AI 建議，可以在 UI 顯示不同顏色
    }
  }
  loadData() {
    this.http.get<any>(`https://localhost:7276/api/Itinerary/${this.itineraryId}`).subscribe(res => {
      this.title = res.itineraryName;
      this.imageUrl = res.itineraryImage;
      this.date = res.startTime;
      this.days = this.mapApiToDays(res.currentVersion?.items || []);
      this.days.forEach(day => {
        day.items.forEach(item => {
          // 如果 AttractionId 是 0 或 null，才需要解析字串
          if (!item.attractionId || item.attractionId === 0) {
            this.parseAiDescription(item);
          }
        });
      });
      if (this.days.length > 0) {
        this.activeDayIndex = this.days[0].day;
      }
    });
  }

  openSearchModal(day: DayPlan) {
    this.showSearchModal = true;
    this.currentAddingDay = day;
    setTimeout(() => {
      if (this.searchInput) {
        const autocomplete = new google.maps.places.Autocomplete(this.searchInput.nativeElement);
        autocomplete.addListener('place_changed', () => {
          const place = autocomplete.getPlace();
          if (place.geometry) {
            this.handlePlaceSelection(place);
            this.showSearchModal = false;
          }
        });
      }
    }, 100);
  }

  handlePlaceSelection(place: any) {
    if (!place || !this.currentAddingDay) return;
    const newItem: ItineraryItem = {
      itemId: 0,
      attractionId: 0,
      dayNumber: this.currentAddingDay.day,
      attractionName: place.name,
      address: place.formatted_address,
      placeId: place.place_id,
      googlePlaceId: place.place_id,
      latitude: place.geometry.location.lat(),
      longitude: place.geometry.location.lng(),
      startTime: '10:00',
      sortOrder: (this.currentAddingDay.items.length + 1) * 100,
      contentDescription: `新增行程`
    };
    this.currentAddingDay.items.push(newItem);
    this.updateSortOrders();
  }

  deleteItem(day: DayPlan, index: number) {
    if (confirm('確定要刪除嗎？')) {
      day.items.splice(index, 1);
      this.updateSortOrders();
    }
  }

  changeItem(event: any) {
    const flattenedItems: any[] = [];
    this.days.forEach(day => {
      day.items.forEach(item => {
        flattenedItems.push({
          AttractionId: item.attractionId || (item as any).AttractionId || 0,
          Name: item.attractionName || (item as any).Name,
          Address: item.address || (item as any).Address,
          Latitude: item.latitude,
          Longitude: item.longitude,
          DayNumber: day.day,
          ContentDescription: item.contentDescription || "無描述",
          PlaceId: item.placeId || item.googlePlaceId || null, // 👈 統一使用 PlaceId
          // 注意：StartTime 的處理見下方第 2 點
          StartTime: this.combineDateAndTime(this.date, item.startTime),
          EndTime: item.endTime ? this.combineDateAndTime(this.date, item.endTime) : null
        });
      });
    });

    const payload = {
      ItineraryId: Number(this.itineraryId),
      VersionNote: '手動修改行程',
      Items: flattenedItems
    };
    console.log("最後發送的 Payload:", payload);
    this.http.post(`https://localhost:7276/api/Itinerary/${this.itineraryId}/save-snapshot`, payload)
      .subscribe(() => alert('修改成功'));
  }

  onDrop(event: CdkDragDrop<ItineraryItem[]>) {
    if (event.previousContainer === event.container) {
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
    } else {
      transferArrayItem(
        event.previousContainer.data,
        event.container.data,
        event.previousIndex,
        event.currentIndex
      );
    }
    this.updateSortOrders();
  }
  combineDateAndTime(baseDate: string, timeStr: string | undefined): string | null {
    if (!timeStr) return null;
    // 1. 處理 baseDate (避免 1970 的關鍵)
    const d = new Date(baseDate);
    if (isNaN(d.getTime())) {
      console.error("無效的基準日期:", baseDate);
      return null;
    }

    // 格式化為 YYYY-MM-DD
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');

    // 2. 處理 timeStr (只取前 5 碼 HH:mm，防止帶入舊的日期字串)
    // 如果 timeStr 是 "2026-03-20T10:00:00"，我們只想要 "10:00"
    let pureTime = "";
    if (timeStr.includes('T')) {
      pureTime = timeStr.split('T')[1].substring(0, 5);
    } else {
      pureTime = timeStr.substring(0, 5);
    }

    const result = `${year}-${month}-${day}T${pureTime}:00`;
    return result;
  }
  private updateSortOrders() {
    this.days.forEach(day => {
      day.items.forEach((item, index) => {
        item.sortOrder = (index + 1) * 100;
      });
    });
  }
}
