import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, OnInit, OnDestroy, AfterViewInit } from '@angular/core';
import { RouterLink } from '@angular/router';

declare var Swiper: any;

@Component({
  selector: 'app-trip-index',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './trip-index.html',
  styleUrl: './trip-index.css',
})
export class TripIndex implements OnInit, OnDestroy, AfterViewInit {
  hotTrips: any[] = [];
  swiperInstance: any;
  private apiUrl = 'https://localhost:7276/api/Trip';

  constructor(private http: HttpClient) { }

  ngOnInit(): void {
    // 直接使用 http 抓取資料
    this.http.get<any[]>(`${this.apiUrl}/Hot?take=8`).subscribe({
      next: (data) => {
        this.hotTrips = data;
        console.log('API Data:', data);

        this.destroySwiper();
        // 延遲時間建議稍微拉長，確保 DOM 渲染完畢
        setTimeout(() => {
          this.initSwiper();
        }, 800);
      },
      error: (err) => console.error('API Error:', err)
    });
  }

  ngAfterViewInit(): void {
    // 介面要求實作，留空即可
  }

  initSwiper() {
    if (typeof Swiper === 'undefined') return;

    const swiperEl = document.querySelector('.mySwiper');
    if (!swiperEl) return;

    try {
      this.swiperInstance = new Swiper(".mySwiper", {
        // 核心功能：自動播放
        autoplay: {
          delay: 2000,                // 3秒切換一次
          disableOnInteraction: false, // 使用者點擊或滑過後，依然繼續自動播放
          pauseOnMouseEnter: true,    // 滑鼠游標移入時暫停，移出後繼續 (增加使用者體驗)
        },

        // 循環播放 (資料至少要大於 slidesPerView 才能動)
        loop: this.hotTrips.length >= 3,

        // 基礎佈局
        slidesPerView: 3,
        spaceBetween: 30,

        // 導覽元件
        navigation: {
          nextEl: ".swiper-button-next",
          prevEl: ".swiper-button-prev",
        },
        pagination: {
          el: ".swiper-pagination",
          clickable: true,
        },

        // 響應式斷點
        breakpoints: {
          320: { slidesPerView: 1, spaceBetween: 10 },
          768: { slidesPerView: 2, spaceBetween: 20 },
          1024: { slidesPerView: 3, spaceBetween: 30 }
        },

        // 重要：監聽 DOM 變化
        observer: true,
        observeParents: true,
      });
      console.log('Swiper 啟動成功，自動播放已開啟');
    } catch (e) {
      console.error('Swiper 初始化失敗：', e);
    }
  }
  // 必須定義這個方法，否則 ngOnInit 會報錯
  destroySwiper() {
    if (this.swiperInstance) {
      if (typeof this.swiperInstance.destroy === 'function') {
        this.swiperInstance.destroy(true, true);
      }
      this.swiperInstance = null;
    }
  }

  // 必須定義這個方法，因為類別實作了 OnDestroy
  ngOnDestroy(): void {
    this.destroySwiper();
  }
}
