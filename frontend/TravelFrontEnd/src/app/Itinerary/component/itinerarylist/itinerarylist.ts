import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, OnInit, signal } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { environment } from '../../../../environments/environment';
export interface ItineraryCard {
  itineraryId: number;
  itineraryName: string;
  itineraryImage: string | null;
  introduction: string | null;
  startTime: string | null;
  endTime: string | null;
  currentStatus: string | null;
}
@Component({
  selector: 'app-itinerarylist',
  imports: [CommonModule, RouterModule],
  templateUrl: './itinerarylist.html',
  styleUrl: './itinerarylist.css',
})
export class Itinerarylist implements OnInit {
  itineraries = signal<ItineraryCard[]>([]);
  isLoading = signal(true);
  errorMsg = signal('');
  baseUrl: string = environment.apiBaseUrl;

  private apiUrl = `${this.baseUrl}/Itinerary/list`;

  constructor(private http: HttpClient, private router: Router) { }
  getTripDays(start: string | null, end: string | null): number {
    if (!start || !end) return 0;
    const diff = new Date(end).getTime() - new Date(start).getTime();
    return Math.max(1, Math.round(diff / (1000 * 60 * 60 * 24)) + 1);
  }

  /**依狀態計算數量*/
  getCountByStatus(status: string): number {
    return this.itineraries().filter(i => this.getComputedStatus(i) === status).length;
  }
  ngOnInit(): void {
    this.fetchItineraries();
  }
  activeFilter = signal<string>('all');

  get filteredItineraries(): ItineraryCard[] {
    const filter = this.activeFilter();
    if (filter === 'all') return this.itineraries();
    return this.itineraries().filter(i => this.getComputedStatus(i) === filter);
  }
  /**設定過濾 */
  setFilter(filter: string): void {
    this.activeFilter.set(filter);
  }
  /**載入行程 */
  fetchItineraries(): void {
    const token = localStorage.getItem('token');
    const headers = new HttpHeaders({ Authorization: `Bearer ${token}` });

    this.http.get<ItineraryCard[]>(this.apiUrl, { headers }).subscribe({
      next: (data) => {
        this.itineraries.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.errorMsg.set('載入行程失敗，請重新登入或稍後再試。');
        this.isLoading.set(false);
        console.error(err);
      }
    });
  }
  /** 跳轉到行程詳細頁面 */
  goToDetail(id: number): void {
    if (!id) {
      console.error('錯誤：行程 ID 為空，無法跳轉。請檢查 API 資料。');
      return;
    }
    this.router.navigate(['/itinerary-detail', id]);
  }
  /**格式化時間 */
  formatDate(dateStr: string | null): string {
    if (!dateStr) return '未設定';
    const d = new Date(dateStr);
    return d.toLocaleDateString('zh-TW', { year: 'numeric', month: '2-digit', day: '2-digit' });
  }


  /** 計算動態狀態 */
  getComputedStatus(item: ItineraryCard): string {
    if (!item.startTime || !item.endTime) return 'draft';
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const start = new Date(item.startTime);
    const end = new Date(item.endTime);
    if (today < start) return 'planned';
    if (today > end) return 'done';
    return 'ongoing';
  }

  /** 狀態標籤對應中文 */
  getStatusLabel(status: string | null): string {
    switch (status) {
      case 'planned': return '已規劃';
      case 'ongoing': return '進行中';
      case 'done': return '已完成';
      default: return '草稿';
    }
  }
  /**設定預設圖片 */
  getFallbackImage(): string {
    return 'https://res.cloudinary.com/dcyrbbv4w/image/upload/v1775030711/itinerary-placeholder_c8jccb.avif'; // 本地預設封面
  }
}
