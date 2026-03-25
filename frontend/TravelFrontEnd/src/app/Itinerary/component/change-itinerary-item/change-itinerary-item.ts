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

  loadData() {
    this.http.get<any>(`https://localhost:7276/api/Itinerary/${this.itineraryId}`).subscribe(res => {
      this.title = res.itineraryName;
      this.imageUrl = res.ItineraryImage;
      this.date = res.startTime;
      this.days = this.mapApiToDays(res.currentVersion?.items || []);
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
        flattenedItems.push({ ...item, dayNumber: day.day });
      });
    });
    const payload = {
      itineraryId: this.itineraryId,
      items: flattenedItems,
      versionNote: '手動修改行程'
    };
    this.http.post(`https://localhost:7276/api/Itinerary/SaveSnapshot`, payload)
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

  private updateSortOrders() {
    this.days.forEach(day => {
      day.items.forEach((item, index) => {
        item.sortOrder = (index + 1) * 100;
      });
    });
  }
}
