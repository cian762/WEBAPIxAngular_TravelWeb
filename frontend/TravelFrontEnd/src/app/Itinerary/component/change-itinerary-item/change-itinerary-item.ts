import { Component, Input, OnInit, inject } from '@angular/core';
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

  title = '';
  date = '';
  imageUrl = '';
  days: DayPlan[] = [];
  constructor(private activateroute: ActivatedRoute) { }
  ngOnInit() {
    this.itineraryId = this.activateroute.snapshot.params['id']
    if (this.itineraryId) {
      this.loadData();
    }
  }
  /**把數據排序並做資料格式轉譯 */
  private mapApiToDays(items: ItineraryItem[]): DayPlan[] {
    if (!items || items.length === 0) return [];

    // 1. 先按時間排序（確保後續分組有序）
    const sortedItems = items.sort((a, b) =>
      new Date(a.startTime || '').getTime() - new Date(b.startTime || '').getTime()
    );

    // 2. 依照日期分組（這裡假設後端有提供日期或從 startTime 判斷）
    const groups = new Map<number, ItineraryItem[]>();

    sortedItems.forEach(item => {
      // 假設後端 DTO 有 Day 欄位，若無，可用日期計算
      const day = 1; // 實務上請依據業務邏輯判斷天數
      if (!groups.has(day)) groups.set(day, []);
      groups.get(day)?.push(item);
    });

    // 3. 轉為 DayPlan 格式
    return Array.from(groups.entries()).map(([dayNumber, dayItems]) => ({
      day: dayNumber,
      items: dayItems
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
    this.http.get<any>(`api/itinerary/${this.itineraryId}`).subscribe(res => {
      this.title = res.itineraryName;
      this.imageUrl = res.ItineraryImage;
      this.date = res.startTime;
      // 假設後端資料結構轉前端
      this.days = this.mapApiToDays(res.currentVersion?.items || []);
    });
  }
  /**新增行程 */
  addItem(_t8: DayPlan) {
    throw new Error('Method not implemented.');
  }
  /**刪除行程 */
  deleteItem(day: DayPlan, index: any) {
    if (confirm('確定要刪除嗎？')) {
      day.items.splice(index, 1);
      this.updateSortOrders();
    }
  }
  onDragStart(_t19: ItineraryItem, _t15: number) {
    throw new Error('Method not implemented.');
  }
  /**修改行程 */
  changeItem(event: any) {
    const file = event.target.files[0];
    if (!file) return;

    const formData = new FormData();
    formData.append('file', file);
    formData.append('upload_preset', 'your_preset'); // Cloudinary 的設定

    // 建議：呼叫後端 API 由後端上傳到 Cloudinary 並回傳網址
    this.http.post<any>(`api/itinerary/${this.itineraryId}/save-snapshot`, formData).subscribe(res => {
      this.imageUrl = res.url; // 拿到網址後更新畫面預覽
      // 同時更新你要送回後端的 DTO 欄位
    });
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
