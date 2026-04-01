import { Mainservice } from './../../service/mainservice';
import { DayItineraryDto, DayPlan } from './../../interface/itinerarymainmodel';
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
import { ItineraryItem } from '../../interface/itinerarymainmodel';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { environment } from '../../../../environments/environment';
import { GoogleMAPservice } from '../../service/google-mapservice';
import { RouterMapComponent } from '../router-map-component/router-map-component';
import { ItineraryMetricscomponent } from '../itinerary-metricscomponent/itinerary-metricscomponent';
import { VersionCompareDialog } from '../version-compare-dialog/version-compare-dialog';
import { ToastService } from '../../service/toast-service';
declare const google: any;

@Component({
  selector: 'app-itinerary-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, CdkDropListGroup, CdkDropList, CdkDrag, RouterModule, RouterMapComponent, ItineraryMetricscomponent, VersionCompareDialog],
  templateUrl: './change-itinerary-item.html',
  styleUrl: './change-itinerary-item.css',
})
export class ItineraryDetailComponent implements OnInit {
  private http = inject(HttpClient);
  baseUrl: string = environment.apiBaseUrl;
  private toast = inject(ToastService);

  @Input() itineraryId!: number;
  @ViewChild('searchInput') searchInput!: ElementRef;
  isExporting = false;
  showSearchModal = false;
  title = '';
  date = '';
  imageUrl = '';
  days: DayPlan[] = [];
  startTime: string = '';
  endDate: string = '';
  introduction: string = '';
  dayTabs: number[] = [];
  /** 目前顯示的天數（預設第1天） */
  activeDayIndex = 1;
  currentDayItinerary?: DayItineraryDto;
  showCompareDialog = false;
  currentVersionId = 0;
  isEditingDescription = false;
  editingDescription = '';
  isReporting = false;
  reportForm = {
    errorType: '地點不存在',
    severityLevel: 'Medium',
    errorMessage: '',
    errorReason: ''
  };
  /** 所有行程總數（地圖圖例用） */
  get totalItems(): number {
    return this.days.reduce((sum, d) => sum + d.items.length, 0);
  }

  private currentAddingDay?: DayPlan;

  constructor(private activateroute: ActivatedRoute, private itineraryService: Mainservice, private mapsService: GoogleMAPservice, private router: Router) { }
  /**建立HOOK */
  ngOnInit() {
    this.itineraryId = Number(this.activateroute.snapshot.params['id']);
    if (this.itineraryId) {
      this.loadData();
    }
  }
  /**開啟報錯視窗 */
  openErrorReport(): void {
    this.isReporting = true;
  }
  /**關閉報錯視窗 */
  closeErrorReport(): void {
    this.isReporting = false;
    this.reportForm = {
      errorType: '地點不存在',
      severityLevel: 'Medium',
      errorMessage: '',
      errorReason: ''
    };
  }
  /**提交報錯到後端 */
  submitErrorReport() {
    const payload = {
      ItineraryID: this.itineraryId,
      VersionID: this.currentVersionId,
      ErrorType: this.reportForm.errorType,
      SeverityLevel: this.reportForm.severityLevel,
      ErrorMessage: this.reportForm.errorMessage,
      ErrorReason: "User Reported", // 或者是您想從前端帶入的其他欄位
      IsConfirmed: false
    };

    this.http.post(`${this.baseUrl}/Itinerary/report`, payload).subscribe({
      next: () => {
        this.toast.success('感謝您的回報，我們會盡快處理！');
        this.closeErrorReport();
      },
      error: (err) => {
        console.error('回報失敗', err);
        this.toast.error('回報失敗，請稍後再試');
      }
    });
  }
  /** 開啟編輯行程描述 */
  openDescriptionEdit(): void {
    this.editingDescription = this.introduction || '';
    this.isEditingDescription = true;
  }
  /**取消行程描述編輯視窗 */
  cancelDescriptionEdit(): void {
    this.isEditingDescription = false;
  }
  /**確認並關閉行程描述編輯視窗 */
  confirmDescriptionEdit(): void {
    if (this.editingDescription === this.introduction) {
      this.isEditingDescription = false;
      return;
    }
    this.updateIntroduction(this.editingDescription);
  }
  /**連接API更新行程描述 */
  private updateIntroduction(newIntro: string) {
    const url = `${this.baseUrl}/Itinerary/${this.itineraryId}/description`;

    this.http.patch(url, { introduction: newIntro }).subscribe({
      next: () => {
        this.introduction = newIntro; // 成功後更新畫面
        this.isEditingDescription = false;

      },
      error: (err) => {
        console.error('更新失敗', err);

      }
    });
  }
  /**載入數據後分別對應到天數 */
  private mapApiToDays(items: ItineraryItem[]): DayPlan[] {
    if (!items || items.length === 0) return [];
    const groups = new Map<number, ItineraryItem[]>();
    items.forEach(item => {
      const d = Number(item.dayNumber) || 1;
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
  /**上傳圖片並即時更改 */
  uploadImage(event: any) {
    const file = event.target.files[0];
    if (!file) return;
    const formDataimg = new FormData();
    formDataimg.append('image', file);
    this.http.post<any>(`${this.baseUrl}/Itinerary/Savephoto/${this.itineraryId}`, formDataimg)
      .subscribe({
        next: (res) => { this.imageUrl = res.url; this.toast.success('封面更新成功！');; },
        error: (err) => { console.error('上傳失敗', err); this.toast.error('上傳失敗，請檢查網絡'); }
      });
  }
  /**開啟過去版本視窗*/
  viewVersions() {
    this.showCompareDialog = true;
  }
  /**數共有幾天數 */
  getCountForDay(dayNum: number): number {
    const dayData = this.days.find(d => d.day === dayNum);
    return dayData ? dayData.items.length : 0;
  }
  /**activeDayIndex 的 setter 改成方法，切換時同步更新 currentDayItinerary */
  selectDay(dayNum: number) {
    this.activeDayIndex = dayNum;
    this.syncCurrentDayItinerary(dayNum);
  }
  /**把DayPlan 轉為DayItineraryDto */
  private syncCurrentDayItinerary(dayNum: number) {
    const dayPlan = this.days.find(d => d.day === dayNum);
    if (!dayPlan) {
      this.currentDayItinerary = undefined;
      return;
    }
    this.currentDayItinerary = {
      dayNumber: dayNum,
      items: dayPlan.items.map((item, index) => ({
        placeId: item.placeId || item.googlePlaceId || '',
        attractionName: item.attractionName,
        address: item.address,
        startTime: item.startTime,
      }))
    };
  }
  /**新增天數的邏輯與呼叫API把時間往後延 */
  addExtraDay() {
    this.http.patch<any>(`${this.baseUrl}/Itinerary/${this.itineraryId}/extend-day`, {})
      .subscribe({
        next: (res) => {
          // 🚩 防禦性檢查：嘗試抓取大小寫可能的欄位
          const newTime = res.newEndTime || res.NewEndTime;

          if (newTime) {
            // 🚩 修正 1：確保賦值
            this.endDate = newTime;

            // 🚩 修正 2：直接在呼叫前印出這兩個值，確認它們真的不一樣
            console.log('計算前:', this.startTime, this.endDate);

            this.generateDayTabs(this.startTime, this.endDate);

            // 🚩 修正 3：手動檢查 dayTabs 長度
            console.log('更新後的 DayTabs:', this.dayTabs);

            setTimeout(() => {
              const lastDay = this.dayTabs[this.dayTabs.length - 1];
              this.activeDayIndex = lastDay;
              // 執行一次確保 days 陣列裡有這一天
              this.getOrCreateDayPlan(lastDay);
            }, 100);
          }
        }
      });
  }
  /**初始載入並呼叫API */
  loadData() {
    this.http.get<any>(`${this.baseUrl}/Itinerary/${this.itineraryId}`).subscribe(res => {
      console.log('API 回傳的原始資料:', res);
      this.title = res.itineraryName;
      this.imageUrl = res.itineraryImage;
      this.startTime = res.startTime || res.StartTime;
      this.endDate = res.endTime || res.EndTime;
      console.log('存入元件後的 startTime:', this.startTime);
      this.introduction = res.introduction || res.Introduction || '';
      this.currentVersionId = res.VersionId || res.versionId || 0;
      this.generateDayTabs(this.startTime, this.endDate);
      this.days = this.mapApiToDays(res.currentVersion?.items || []);
      console.log('第一筆 item 原始結構:', res.currentVersion?.items?.[0]);
      if (this.days.length > 0) {
        this.activeDayIndex = this.days[0].day;
      } else {
        this.activeDayIndex = 1;
      }
      this.syncCurrentDayItinerary(this.activeDayIndex);
      console.log(this.currentVersionId);
      this.syncCurrentDayItinerary(this.activeDayIndex);
    });
  }
  /**顯示空天數並且可自己新增行程 */
  getOrCreateDayPlan(dayNum: number): DayPlan {
    // 先找看看 days 陣列有沒有這一天
    let dayPlan = this.days.find(d => d.day === dayNum);

    // 如果找不到（代表是剛新增的空天數）
    if (!dayPlan) {
      dayPlan = {
        day: dayNum,
        items: []
      };
      // 將這個新的空天數物件加入 days 陣列，並排序
      this.days.push(dayPlan);
      this.days.sort((a, b) => a.day - b.day);
    }

    return dayPlan;
  }
  /**生成旁邊的空天數 */
  generateDayTabs(start: string, end: string) {
    const finalStart = start || this.startTime;
    if (!finalStart || !end) {
      console.error('無法計算天數：開始或結束日期缺失', { finalStart, end });
      return;
    }
    const s = new Date(start);
    const e = new Date(end);

    // 🚩 防呆：重設日期時間為 00:00:00，避免因為小時數導致計算不滿一天
    s.setHours(0, 0, 0, 0);
    e.setHours(0, 0, 0, 0);

    const diffTime = e.getTime() - s.getTime();
    const diffDays = Math.floor(diffTime / (1000 * 60 * 60 * 24)) + 1;
    // 🚩 重新賦值陣列，觸發 Angular 變更偵測
    console.log('算出的天數:', diffDays);
    this.dayTabs = Array.from({ length: diffDays > 0 ? diffDays : 1 }, (_, i) => i + 1);
  }
  /**開啟新增行程的視窗 */
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
  /**新增行程並列成資料 */
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
      contentDescription: `${place.name}`
    };
    this.currentAddingDay.items.push(newItem);
    this.updateSortOrders();
  }
  /**編輯行程名稱 */
  editName(event: Event, item: any) {
    event.stopPropagation();
    item.isEditing = true;
    // 暫存目前名稱，讓使用者看到現有值可以修改
    item.editingName = item.editingName || item.attractionName || item.contentDescription || '';
  }
  /**確認修改行程名稱 */
  confirmEdit(item: any): void {
    // 把暫存名稱存下來，但不呼叫 API
    // editingName 已經透過 ngModel 雙向綁定更新了
    item.isEditing = false;
    // 標記此 item 已被修改，儲存時可以識別哪些需要寫 DB
    item.isDirty = true;
  }
  /**取消修改行程名稱 */
  cancelEdit(item: any): void {
    // 取消：清除 input，回到原本顯示
    item.isEditing = false;
    // editingName 保留之前已確認的值（若有的話），不清除
  }
  /**刪除物件 */
  deleteItem(day: DayPlan, index: number) {
    if (confirm('確定要刪除嗎？')) {
      day.items.splice(index, 1);
      this.updateSortOrders();
    }
  }
  /**刪除整筆行程 */
  deleteItinerary() {
    if (confirm('確定要刪除整個行程嗎？')) {
      this.http.delete(`${this.baseUrl}/Itinerary/${this.itineraryId}`)
        .subscribe(() => {
          this.toast.success('行程已刪除');
          this.router.navigate(['/Itinerarylist']);
        });
    }
  }
  /**把ITEM物件扁平化 */
  changeItem(event: any) {
    const flattenedItems: any[] = [];
    this.days.forEach(day => {
      day.items.forEach(item => {
        flattenedItems.push({
          AttractionId: item.attractionId || (item as any).AttractionId || 0,
          Name: (item as any).editingName || item.attractionName || (item as any).Name,
          Address: item.address || (item as any).Address,
          Latitude: item.latitude,
          Longitude: item.longitude,
          DayNumber: day.day,
          ContentDescription: (item as any).editingName || item.contentDescription || "無描述",
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
    this.http.post(`${this.baseUrl}/Itinerary/${this.itineraryId}/save-snapshot`, payload)
      .subscribe(() => this.toast.success('修改成功'));
  }
  /**判斷拖拉事件 */
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
    this.mapsService.clearDayCache(this.itineraryId, this.activeDayIndex);
    this.syncCurrentDayItinerary(this.activeDayIndex);
  }
  /**把時間結合成後端能接受的狀態 */
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
  /**重新排序 */
  private updateSortOrders() {
    this.days.forEach(day => {
      day.items.forEach((item, index) => {
        item.sortOrder = (index + 1) * 100;
      });
    });
  }
  /**下載PDF */
  exportPdf(): void {
    if (this.isExporting) return;
    this.isExporting = true;

    this.http.get(
      `https://localhost:7276/api/Itinerary/${this.itineraryId}/export`,
      { responseType: 'blob' }   // ✅ 關鍵：告訴 HttpClient 回傳的是二進位檔
    ).subscribe({
      next: (blob) => {
        // 建立暫時的下載連結並觸發點擊
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `${this.title || '行程'}.pdf`;  // 檔名用行程標題
        a.click();

        // 清除暫時 URL，避免記憶體洩漏
        window.URL.revokeObjectURL(url);
        this.isExporting = false;
      },
      error: (err) => {
        console.error('匯出失敗', err);
        this.toast.error('PDF 匯出失敗，請稍後再試');
        this.isExporting = false;
      }
    });
  }
}
