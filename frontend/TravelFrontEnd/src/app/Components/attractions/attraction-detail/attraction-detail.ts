import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { AttractionService } from '../attraction.service';
import { SafeUrlPipe } from '../safe-url.pipe';
import { Attraction } from '../attraction.models';
import { TicketSectionComponent } from '../ticket-section/ticket-section';
import Swal from 'sweetalert2';
import { FormsModule } from '@angular/forms';
import { ReviewItem } from '../attraction.service'; // 確認路徑
import { AuthService } from '../../../Member/services/auth.service';
// 路徑確認：auth.service 在 app/Member/services/ 下


@Component({
  selector: 'app-attraction-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, SafeUrlPipe, TicketSectionComponent, FormsModule],
  templateUrl: './attraction-detail.html',
  styleUrls: ['./attraction-detail.css']
})
export class AttractionDetailComponent implements OnInit {
  attraction: Attraction | null = null;
  loading = true;
  activeTab = 'feature';
  currentImgIdx = 0;
  nearbyAttractions: {
    attractionId: number;
    name: string;
    address: string | null;
    mainImage: string | null;
    distanceKm: number;
  }[] = [];

  relatedTickets: {
    attractionId: number;
    name: string;
    mainImage: string | null;
    ticketTitle: string | null;
    ticketPrice: number | null;
    originalPrice: number | null;
    ticketTypeName: string | null;
    overlapCount: number;
  }[] = [];

  // [YJ] 評論
  reviews: ReviewItem[] = [];
  avgRating = 0;
  reviewCount = 0;
  canWriteReview = false;
  reviewReason = '';
  showReviewForm = false;
  submitting = false;
  newReview = { rating: 5, title: '', comment: '' };


  tabs = [
    { key: 'feature', label: '景點特色', icon: '🏞️' },
    { key: 'transport', label: '如何前往', icon: '🚌' },
    { key: 'tickets', label: '售票區', icon: '🎟️' },
    { key: 'nearby', label: '周邊資訊', icon: '📍' },
  ];

  // 天氣 / AQI（預設值，載入完成前顯示）
  weather = {
    temp: '--' as number | string,
    rain: '--' as number | string,
    aqi: '--' as number | string,
    aqiLabel: '載入中',
    aqiEmoji: '😐',
    weatherCode: 0,
    weatherEmoji: '⛅',
  };

  // regionName → [lat, lng]（縣市中心座標）
  private readonly regionCoords: Record<string, [number, number]> = {
    '臺北市': [25.04, 121.51], '台北市': [25.04, 121.51],
    '新北市': [25.01, 121.46],
    '基隆市': [25.13, 121.74],
    '桃園市': [24.99, 121.30],
    '新竹市': [24.80, 120.97],
    '新竹縣': [24.84, 121.01],
    '苗栗縣': [24.56, 120.82],
    '台中市': [24.15, 120.67], '臺中市': [24.15, 120.67],
    '彰化縣': [24.07, 120.54],
    '南投縣': [23.96, 120.97],
    '雲林縣': [23.71, 120.43],
    '嘉義市': [23.48, 120.45],
    '嘉義縣': [23.46, 120.44],
    '台南市': [23.00, 120.21], '臺南市': [23.00, 120.21],
    '高雄市': [22.63, 120.30],
    '屏東縣': [22.55, 120.55],
    '宜蘭縣': [24.70, 121.74],
    '花蓮縣': [23.99, 121.60],
    '台東縣': [22.75, 121.15], '臺東縣': [22.75, 121.15],
    '澎湖縣': [23.57, 119.58],
    '金門縣': [24.44, 118.32],
    '連江縣': [26.16, 119.95],
  };

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private svc: AttractionService,
    private http: HttpClient,
    private authService: AuthService   // ← 加這行
  ) { }

  ngOnInit(): void {
    // 讀 ?tab=tickets 自動切換 Tab（從購物車編輯按鈕導回來時用）
    this.route.queryParams.subscribe(params => {
      if (params['tab']) this.activeTab = params['tab'];
    });

    this.route.paramMap.subscribe(p => {
      const id = Number(p.get('id'));
      if (id) {
        this.svc.getAttractionById(id).subscribe(data => {
          this.attraction = data;
          this.loading = false;
          if (data) {
            // ← 從 localStorage 還原按讚狀態
            const liked = localStorage.getItem(`liked_attraction_${id}`);
            if (liked && this.attraction) this.attraction.isLiked = true;

            this.loadWeather(data);
            this.svc.getNearbyAttractions(id).subscribe(list => { this.nearbyAttractions = list; });
            this.svc.getRelatedTickets(id).subscribe(list => { this.relatedTickets = list; });

            // [YJ] 載入評論
            this.svc.getAttractionReviews(id).subscribe(res => {
              this.reviews = res.reviews;
              this.avgRating = res.averageRating;
              this.reviewCount = res.totalCount;
            });

            // [YJ] 確認是否可評論
            if (this.authService.isLoggedIn()) {
              this.svc.canReview(id).subscribe(res => {
                this.canWriteReview = res.canReview;
                this.reviewReason = res.reason;
              });
            }
          }
        });
      }
    });
  }

  /** 根據景點座標取天氣 + AQI，沒有座標才 fallback 到縣市中心 */
  private loadWeather(attraction: Attraction): void {
    // 優先用景點自己的 lat/lng，沒有才查縣市對照表
    let lat: number;
    let lng: number;

    if (attraction.latitude && attraction.longitude) {
      lat = attraction.latitude;
      lng = attraction.longitude;
    } else {
      const regionName = attraction.regionName ?? '';
      [lat, lng] = this.regionCoords[regionName] ?? [25.04, 121.51];
    }

    // ── 天氣 API ──────────────────────────────────────
    const weatherUrl =
      `https://api.open-meteo.com/v1/forecast` +
      `?latitude=${lat}&longitude=${lng}` +
      `&current=temperature_2m,precipitation_probability,weathercode` +
      `&timezone=Asia%2FTaipei`;

    this.http.get<any>(weatherUrl).subscribe({
      next: (res) => {
        const cur = res.current;
        this.weather.temp = Math.round(cur.temperature_2m);
        this.weather.rain = cur.precipitation_probability ?? '--';
        this.weather.weatherCode = cur.weathercode ?? 0;
        this.weather.weatherEmoji = this.codeToEmoji(cur.weathercode ?? 0);
      },
      error: () => {
        this.weather.temp = '--';
        this.weather.rain = '--';
      }
    });

    // ── 空氣品質 API ──────────────────────────────────
    const aqiUrl =
      `https://air-quality-api.open-meteo.com/v1/air-quality` +
      `?latitude=${lat}&longitude=${lng}` +
      `&current=european_aqi` +
      `&timezone=Asia%2FTaipei`;

    this.http.get<any>(aqiUrl).subscribe({
      next: (res) => {
        const aqi = Math.round(res.current?.european_aqi ?? 0);
        this.weather.aqi = aqi;
        const { label, emoji } = this.aqiToLabel(aqi);
        this.weather.aqiLabel = label;
        this.weather.aqiEmoji = emoji;
      },
      error: () => {
        this.weather.aqi = '--';
        this.weather.aqiLabel = '無資料';
        this.weather.aqiEmoji = '😐';
      }
    });
  }

  /** WMO weathercode → emoji */
  private codeToEmoji(code: number): string {
    if (code === 0) return '☀️';
    if (code <= 2) return '⛅';
    if (code <= 3) return '☁️';
    if (code <= 49) return '🌫️';
    if (code <= 69) return '🌧️';
    if (code <= 79) return '🌨️';
    if (code <= 99) return '⛈️';
    return '🌤️';
  }

  /** EU AQI 數值 → 中文標籤 + emoji */
  private aqiToLabel(aqi: number): { label: string; emoji: string } {
    if (aqi <= 20) return { label: '優良', emoji: '😊' };
    if (aqi <= 40) return { label: '良好', emoji: '😊' };
    if (aqi <= 60) return { label: '普通', emoji: '😐' };
    if (aqi <= 80) return { label: '不良', emoji: '😷' };
    if (aqi <= 100) return { label: '很差', emoji: '😷' };
    return { label: '危險', emoji: '⛔' };
  }

  get mainImage(): string {
    if (this.attraction?.images?.length) {
      return `https://localhost:7285${this.attraction.images[this.currentImgIdx]}`;
    }
    return 'https://placehold.co/600x400?text=No+Image';
  }

  get imageCount(): number { return this.attraction?.images?.length ?? 0; }

  prevImg(): void {
    this.currentImgIdx = (this.currentImgIdx - 1 + this.imageCount) % this.imageCount;
  }
  nextImg(): void {
    this.currentImgIdx = (this.currentImgIdx + 1) % this.imageCount;
  }

  toggleLike(): void {
    if (!this.attraction) return;
    const id = this.attraction.attractionId;

    this.svc.toggleLike(id).subscribe({
      next: (res) => {
        if (!this.attraction) return;
        this.attraction.likeCount = res.likeCount;

        if (res.liked) {
          // 按讚成功
          this.attraction.isLiked = true;
          localStorage.setItem(`liked_attraction_${id}`, '1');
        } else {
          // 取消按讚
          this.attraction.isLiked = false;
          localStorage.removeItem(`liked_attraction_${id}`);
        }
      },
      error: () => {
        Swal.fire({ icon: 'error', title: '操作失敗', text: '請稍後再試', timer: 2000 });
      }
    });
  }



  goToNearby(attractionId: number): void {
    this.router.navigate(['/attractions/detail', attractionId]);
  }

  goToRelated(attractionId: number): void {
    this.router.navigate(['/attractions/detail', attractionId]);
  }

  openNav(): void {
    if (!this.attraction) return;
    window.open(
      `https://www.google.com/maps/dir/?api=1&destination=${this.attraction.latitude},${this.attraction.longitude}`,
      '_blank'
    );
  }

  get mapUrl(): string {
    const lat = this.attraction?.latitude ?? 25.0;
    const lng = this.attraction?.longitude ?? 121.5;
    return `https://maps.google.com/maps?q=${lat},${lng}&z=15&output=embed`;
  }

  /** 將介紹文字依換行切成段落陣列 */
  splitParagraphs(text: string): string[] {
    return text.split(/\n+/).map(s => s.trim()).filter(s => s.length > 0);
  }

  // [YJ] 星星陣列（顯示用）
  starArray(n: number): number[] {
    return Array(Math.round(n)).fill(0);
  }

  emptyStarArray(n: number): number[] {
    return Array(5 - Math.round(n)).fill(0);
  }

  // [YJ] 送出評論
  submitReview(): void {
    if (!this.attraction) return;
    if (!this.newReview.title.trim() || !this.newReview.comment.trim()) {
      Swal.fire({ icon: 'warning', title: '請填寫標題與內容', timer: 2000, showConfirmButton: false });
      return;
    }
    this.submitting = true;
    this.svc.submitReview({
      attractionId: this.attraction.attractionId,
      rating: this.newReview.rating,
      title: this.newReview.title,
      comment: this.newReview.comment
    }).subscribe({
      next: () => {
        Swal.fire({ icon: 'success', title: '評論送出成功！', timer: 2000, showConfirmButton: false });
        this.showReviewForm = false;
        this.canWriteReview = false;
        this.newReview = { rating: 5, title: '', comment: '' };
        // 重新載入評論
        this.svc.getAttractionReviews(this.attraction!.attractionId).subscribe(res => {
          this.reviews = res.reviews;
          this.avgRating = res.averageRating;
          this.reviewCount = res.totalCount;
        });
      },
      error: () => {
        this.submitting = false;
        Swal.fire({ icon: 'error', title: '送出失敗', text: '請稍後再試', timer: 2000, showConfirmButton: false });
      }
    });
  }


  // [YJ] 點擊「留下評價」按鈕
  onReviewBtnClick(): void {
    if (!this.authService.isLoggedIn()) {
      Swal.fire({ icon: 'warning', title: '請先登入', text: '登入後即可留下評價', timer: 2000, showConfirmButton: false });
      return;
    }
    if (this.reviewReason === 'already_reviewed') {
      Swal.fire({ icon: 'info', title: '您已評論過此景點', timer: 2000, showConfirmButton: false });
      return;
    }
    if (this.reviewReason === 'not_purchased') {
      Swal.fire({ icon: 'warning', title: '需購買票券才能評論', timer: 2000, showConfirmButton: false });
      return;
    }
    this.showReviewForm = true;
  }
}
