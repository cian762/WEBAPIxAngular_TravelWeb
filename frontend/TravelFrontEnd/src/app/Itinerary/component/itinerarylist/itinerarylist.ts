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

  ngOnInit(): void {
    this.fetchItineraries();
  }

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

  goToDetail(id: number): void {
    if (!id) {
      console.error('錯誤：行程 ID 為空，無法跳轉。請檢查 API 資料。');
      return;
    }
    this.router.navigate(['/itinerary-detail', id]);
  }

  formatDate(dateStr: string | null): string {
    if (!dateStr) return '未設定';
    const d = new Date(dateStr);
    return d.toLocaleDateString('zh-TW', { year: 'numeric', month: '2-digit', day: '2-digit' });
  }

  getStatusLabel(status: string | null): string {
    switch (status) {
      case 'draft': return '草稿';
      case 'planned': return '已規劃';
      case 'ongoing': return '進行中';
      case 'done': return '已完成';
      default: return status ?? '未知';
    }
  }

  getFallbackImage(): string {
    return 'assets/images/itinerary-placeholder.jpg'; // 本地預設封面
  }
}
