import { Mainservice } from './../../service/mainservice';
import { DayItineraryDto, DayPlan } from './../../interface/itinerarymainmodel';
import { Component, ElementRef, Input, OnInit, ViewChild, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import Swal from 'sweetalert2'
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
    Swal.fire({
      title: '確定要刪除此地點嗎？',
      text: "這將會從當天行程中移除該項目。",
      icon: 'question', // 使用問題圖示，比警告圖示 (warning) 輕微一點
      showCancelButton: true,
      confirmButtonColor: '#3085d6', // 這裡可以用藍色，因為是微調行程
      cancelButtonColor: '#aaa',
      confirmButtonText: '確定移除',
      cancelButtonText: '取消',
      backdrop: `rgba(0,0,0,0.2) blur(2px)` // 輕微霧化
    }).then((result) => {
      if (result.isConfirmed) {
        // 執行原本的刪除邏輯
        day.items.splice(index, 1);
        this.updateSortOrders();

        // 因為是前端操作，可以快速給一個輕量的 Toast 提示
        this.toast.success('已移除該地點');

        // 同步地圖狀態（如果需要的話）
        this.mapsService.clearDayCache(this.itineraryId, this.activeDayIndex);
        this.syncCurrentDayItinerary(this.activeDayIndex);
      }
    });
  }
  /**刪除整筆行程 */
  deleteItinerary() {
    Swal.fire({
      title: '確定要刪除整個行程嗎？',
      text: "刪除後將無法還原此內容！",
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#d33',     // 建議使用紅色對應「刪除」
      cancelButtonColor: '#3085d6',    // 取消按鈕使用藍色或灰色
      confirmButtonText: '確定刪除',
      cancelButtonText: '我再想想',
      reverseButtons: true,            // 讓「取消」在左，「確定」在右，符合一般習慣
      backdrop: `rgba(0,0,0,0.4) blur(4px)` // 加上霧化效果連動
    }).then((result) => {
      // 只有當使用者按下「確定刪除」時才執行
      if (result.isConfirmed) {
        this.http.delete(`${this.baseUrl}/Itinerary/${this.itineraryId}`)
          .subscribe({
            next: () => {
              // 刪除成功後，可以用 Swal 做一個精美的小通知再跳轉
              Swal.fire({
                title: '已刪除！',
                text: '您的行程已成功移除。',
                icon: 'success',
                timer: 1500,
                showConfirmButton: false
              });

              this.router.navigate(['/Itinerarylist']);
            },
            error: (err) => {
              console.error('刪除失敗', err);
              this.toast.error('刪除失敗，請稍後再試');
            }
          });
      }
    });
  }
  /** 依天數算出對應日期字串 */
getDateForDay(dayNumber: number): string {
  if (!this.startTime) return '';
  const d = new Date(this.startTime);
  d.setDate(d.getDate() + (dayNumber - 1));
  return d.toISOString();
}
  /**把ITEM物件扁平化 */
  changeItem(event: any) {
    const flattenedItems: any[] = [];
  this.days.forEach(day => {
    // 算出這一天對應的實際日期（startTime + (day - 1) 天）
    const baseDateForDay = this.getDateForDay(day.day);

    day.items.forEach(item => {
      flattenedItems.push({
        AttractionId: item.attractionId || 0,
        Name: (item as any).editingName || item.attractionName,
        Address: item.address,
        Latitude: item.latitude,
        Longitude: item.longitude,
        DayNumber: day.day,
        ContentDescription: (item as any).editingName || item.contentDescription || '無描述',
        PlaceId: item.placeId || item.googlePlaceId || null,
        StartTime: this.combineDateAndTime(baseDateForDay, item.startTime),
        EndTime: this.combineDateAndTime(baseDateForDay, item.endTime || item.startTime)
      });
    });
  });

    const payload = {
      ItineraryId: Number(this.itineraryId),
      VersionNote: '手動修改行程',
      Items: flattenedItems
    };
    console.log("最後發送的 Payload:", payload);
    this.http.post<{success: boolean; message: string; versionId: number}>(`${this.baseUrl}/Itinerary/${this.itineraryId}/save-snapshot`, payload)
      .subscribe({next: (res) => {
        this.currentVersionId = res.versionId; // 更新元件上的版本 ID

        this.toast.success('修改成功，正在重新分析...');

        // 用新版本 ID 觸發分析（fire-and-forget，分析完成不需等待）
        this.http.get(`${this.baseUrl}/Itinerary/${this.itineraryId}/versions/${res.versionId}/analysis`)
          .subscribe({
            next: () => this.toast.success('AI 分析已更新'),
            error: () => { /* 分析失敗不影響主流程，靜默處理 */ }
          });
      },
      error: () => this.toast.error('儲存失敗，請稍後再試')});
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
  // ===== TIME PICKER 相關方法 =====

  /**
   * 從 startTime 字串中提取純 HH:mm，
   * 相容 "10:00"、"2026-03-20T10:00:00" 兩種格式。
   */
  getPureTime(timeStr: string | undefined): string {
    if (!timeStr) return '';
    if (timeStr.includes('T')) {
      return timeStr.split('T')[1].substring(0, 5);
    }
    return timeStr.substring(0, 5);
  }

  /**
   * 格式化顯示用時間（card 上的時間標籤）。
   * 輸出範例：「10:30」
   */
  formatDisplayTime(timeStr: string | undefined): string {
    return this.getPureTime(timeStr);
  }

  /**
   * 開啟或關閉指定 item 的 inline timepicker，
   * 同時關閉同一天其他 item 已開啟的 timepicker（一次只開一個）。
   */
  toggleTimePicker(item: any, currentDay: DayPlan, event: Event): void {
    event.stopPropagation();
    const wasOpen = item.isTimePickerOpen;
    // 先關掉全部
    currentDay.items.forEach(i => (i as any).isTimePickerOpen = false);
    // 若原本是關的就開起來
    if (!wasOpen) {
      (item as any).isTimePickerOpen = true;
    }
  }

  /**
   * 使用者在 timepicker 輸入時間時觸發：
   * 1. 同步更新 startTime / endTime（兩者相同）
   * 2. 依照時間重新排序當天 items
   */
  onTimeChange(item: any, currentDay: DayPlan, timeValue: string): void {
    if (!timeValue) return;
    item.startTime = timeValue;   // 純 HH:mm，送出時 combineDateAndTime 會補上日期
    item.endTime = timeValue;     // startTime == endTime，避免 NULL
    item.isDirty = true;
  }

  /**
   * 點「✓ 確認」關閉 timepicker。
   */
  confirmTimePicker(item: any, currentDay: DayPlan): void {
    // 若使用者沒改時間，補一個預設值避免 NULL
    if (!item.startTime) {
      item.startTime = '09:00';
      item.endTime = '09:00';
    }
    currentDay.items.sort((a, b) => {
      const ta = this.getPureTime((a as any).startTime) || '00:00';
      const tb = this.getPureTime((b as any).startTime) || '00:00';
      return ta.localeCompare(tb);
    });

    this.updateSortOrders();
    this.syncCurrentDayItinerary(this.activeDayIndex);
    (item as any).isTimePickerOpen = false;
  }

  /**下載PDF */
  exportPdf(): void {
    if (this.isExporting) return;
    this.isExporting = true;

    this.http.get(
      `${this.baseUrl}/Itinerary/${this.itineraryId}/export`,
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
