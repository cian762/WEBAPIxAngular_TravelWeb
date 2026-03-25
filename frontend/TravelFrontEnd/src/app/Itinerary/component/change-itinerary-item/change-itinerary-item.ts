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

  /**輸入ID */
  @Input() itineraryId!: number;
  @ViewChild('searchInput') searchInput!: ElementRef;
  showSearchModal = false;
  title = '';
  date = '';
  imageUrl = '';
  days: DayPlan[] = [];
  private currentAddingDay?: DayPlan;
  constructor(private activateroute: ActivatedRoute) { }
  ngOnInit() {
    this.itineraryId = Number(this.activateroute.snapshot.params['id'])
    console.log(this.itineraryId);
    if (this.itineraryId) {
      this.loadData();
    }
  }
  /**把數據排序並做資料格式轉譯 */
  private mapApiToDays(items: ItineraryItem[]): DayPlan[] {
    if (!items || items.length === 0) return [];
    const groups = new Map<number, ItineraryItem[]>();

    items.forEach(item => {
      // 確保這裡的欄位名稱 (dayNumber) 與後端 JSON 回傳的大小寫一致
      const d = item.dayNumber || 1;
      if (!groups.has(d)) groups.set(d, []);
      groups.get(d)?.push(item);
    });

    return Array.from(groups.entries())
      .sort(([a], [b]) => a - b) // 按天數由小到大排序
      .map(([dayNumber, dayItems]) => ({
        day: dayNumber,
        items: dayItems.sort((a, b) => (a.sortOrder || 0) - (b.sortOrder || 0))
      }));
  }
  /**上傳圖片 */
  uploadImage(event: any) {
    const file = event.target.files[0];
    if (!file) return;

    const formDataimg = new FormData();
    formDataimg.append('image', file);

    // 呼叫剛剛寫的控制器端點
    this.http.post<any>(`https://localhost:7276/api/Itinerary/Savephoto/${this.itineraryId}`, formDataimg)
      .subscribe({
        next: (res) => {
          // 成功後，前端變數 imageUrl 更新，HTML 會自動重新渲染背景圖
          this.imageUrl = res.url;
          alert('封面更新成功！');
        },
        error: (err) => {
          console.error('上傳失敗', err);
          alert('上傳失敗，請檢查網絡');
        }
      });
  }
  /**載入API的數據 */
  loadData() {
    this.http.get<any>(`https://localhost:7276/api/Itinerary/${this.itineraryId}`).subscribe(res => {
      this.title = res.itineraryName;
      this.imageUrl = res.ItineraryImage;
      this.date = res.startTime;
      // 假設後端資料結構轉前端
      this.days = this.mapApiToDays(res.currentVersion?.items || []);
    });
  }
  /**新增行程 */
  openSearchModal(day: DayPlan) {
    this.showSearchModal = true;
    this.currentAddingDay = day;

    setTimeout(() => {
      if (this.searchInput) {
        const autocomplete = new google.maps.places.Autocomplete(this.searchInput.nativeElement);

        autocomplete.addListener('place_changed', () => {
          const place = autocomplete.getPlace();
          if (place.geometry) {
            // 選定地點後的邏輯
            this.handlePlaceSelection(place);
            this.showSearchModal = false; // 關閉彈窗
          }
        });
      }
    }, 100);
  }
  handlePlaceSelection(place: any) {
    if (!place || !this.currentAddingDay) {
      return;
    }
    if (place) {
      const newItem: ItineraryItem = {
        itemId: 0,
        attractionId: 0,
        dayNumber: this.currentAddingDay.day,
        // 新地點為 0，由後端判斷
        attractionName: place.name,
        address: place.formatted_address,
        placeId: place.place_id,
        latitude: place.geometry.location.lat(),
        longitude: place.geometry.location.lng(),
        startTime: '10:00', // 預設時間
        sortOrder: (this.currentAddingDay.items.length + 1) * 100,
        contentDescription: `新增行程`
      };
      this.currentAddingDay.items.push(newItem);
      this.updateSortOrders();
    }
    console.log('選中的地點：', place.name);
  }
  /**刪除行程 */
  deleteItem(day: DayPlan, index: any) {
    if (confirm('確定要刪除嗎？')) {
      day.items.splice(index, 1);
      this.updateSortOrders();
    }
  }

  /**修改行程 */
  changeItem(event: any) {
    // 將 days 拍平成後端要的 List<Item>
    const flattenedItems: any[] = [];
    this.days.forEach(day => {
      day.items.forEach(item => {
        flattenedItems.push({
          ...item,
          dayNumber: day.day // 確保帶上正確的天數
        });
      });
    });

    const payload = {
      itineraryId: this.itineraryId,
      items: flattenedItems,
      versionNote: "手動修改行程"
    };

    this.http.post(`https://localhost:7276/api/Itinerary/SaveSnapshot`, payload)
      .subscribe(res => alert('修改成功'));
  }

  /**處理拖拉後的 SortOrder 更新*/
  onDrop(event: CdkDragDrop<ItineraryItem[]>) {
    if (event.previousContainer === event.container) {
      // 同一天內移動
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
    } else {
      // 跨天移動
      transferArrayItem(
        event.previousContainer.data,
        event.container.data,
        event.previousIndex,
        event.currentIndex
      );
    }
    this.updateSortOrders();
  }
  /**更新排序 */
  private updateSortOrders() {
    this.days.forEach(day => {
      day.items.forEach((item, index) => {
        // 邏輯：第一筆 100, 第二筆 200, 依此類推
        item.sortOrder = (index + 1) * 100;
      });
    });
  }
}
