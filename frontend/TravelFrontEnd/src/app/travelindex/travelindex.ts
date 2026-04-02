import { Component, OnInit, OnDestroy } from '@angular/core';
import { TripIndex } from "../trip/component/trip-index/trip-index";
import { Router, RouterLink } from '@angular/router';
import { HeroSection } from "../Itinerary/component/hero-section/hero-section";
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { GlobalSearchService } from '../Services/global-search-service';
import { SearchBridge } from '../Services/search-bridge';
import { debounceTime, distinctUntilChanged, switchMap } from 'rxjs';
import { CommonModule } from '@angular/common';
import { GlobalSearch } from "../global-search/global-search";
import { ActivityIndexCard } from "../Activity/Component/activity-index-card/activity-index-card";
//[YJ] 景點 Service 與 Model，供熱門景點區塊使用
import { AttractionService } from '../Components/attractions/attraction.service';
import { Attraction } from '../Components/attractions/attraction.models';

@Component({
  selector: 'app-travelindex',
  imports: [TripIndex, RouterLink, HeroSection, ReactiveFormsModule, CommonModule, GlobalSearch, ActivityIndexCard],
  templateUrl: './travelindex.html',
  styleUrl: './travelindex.css',
  template: `<input [formControl]="searchControl" placeholder="搜景點、活動...">`
})
export class Travelindex implements OnInit, OnDestroy {
  searchControl = new FormControl('');
  suggestions: string[] = []; // 存放純文字建議
  showSuggestions = false;    // 控制下拉選單顯示



  // ── [YJ] 景點區塊 ──────────────────────────────
  topRegions: { regionId: number; regionName: string }[] = [];  // 五大頂層地區清單（北/中/南/東/離島），由 API 動態載入
  currentRegionIdx = 0;  // 目前顯示第幾個地區（index）
  regionCache = new Map<number, Attraction[]>();  // 快取各地區已載入的景點，避免重複打 API
  currentAttractions: Attraction[] = [];  // 目前畫面上顯示的 4 筆景點
  private autoSlideTimer: any;  // 自動輪播計時器 handle


  constructor(
    private searchService: GlobalSearchService, // 負責去後端搬貨
    private searchBridge: SearchBridge,           // 負責把貨傳給子元件
    private attractionSvc: AttractionService,   //  [YJ] 景點 Service
    private router: Router                       //[YJ] 路由，用於點擊卡片跳轉
  ) { }


  get currentRegionName(): string {
    return this.topRegions[this.currentRegionIdx]?.regionName ?? '';
  }

  // ── [YJ] 景點地區輪播邏輯 ──────────────────────────────
  loadRegionAttractions(regionId: number): void {
    if (this.regionCache.has(regionId)) {
      this.currentAttractions = this.regionCache.get(regionId)!;
      return;
    }
    this.attractionSvc.getAttractions({ regionId }).subscribe(data => {
      const top4 = data
        .sort((a, b) => (b.viewCount ?? 0) - (a.viewCount ?? 0))
        .slice(0, 4);
      this.regionCache.set(regionId, top4);
      this.currentAttractions = top4;
    });
  }
  switchRegion(idx: number, regionId: number): void {
    this.currentRegionIdx = idx;
    this.loadRegionAttractions(regionId);
    this.resetAutoSlide(); // 點擊後重置計時
  }
  private startAutoSlide(): void {
    this.autoSlideTimer = setInterval(() => {
      if (this.topRegions.length === 0) return;
      this.currentRegionIdx = (this.currentRegionIdx + 1) % this.topRegions.length;
      this.loadRegionAttractions(this.topRegions[this.currentRegionIdx].regionId);
    }, 4000);
  }
  private resetAutoSlide(): void {
    clearInterval(this.autoSlideTimer);
    this.startAutoSlide();
  }
  ngOnDestroy(): void {
    clearInterval(this.autoSlideTimer);
  }
  //景點結束


  ngOnInit() {
    // 監聽打字：專門用來抓「提示詞」
    this.searchControl.valueChanges.pipe(
      debounceTime(200),
      distinctUntilChanged(),
      switchMap(term => {
        if (!term || term.trim().length < 1) {
          this.suggestions = [];
          this.showSuggestions = false;
          return [];
        }
        // 呼叫那支「純文字建議」的 API
        return this.searchService.getSuggestions(term);
      })
    ).subscribe(data => {
      this.suggestions = data;
      this.showSuggestions = data.length > 0;
    });

    //[YJ] 載入五大地區，預設顯示第一區並啟動自動輪播
    this.attractionSvc.getTopRegions().subscribe(regions => {
      this.topRegions = regions.filter(r => r.regionName !== '待定');
      if (this.topRegions.length > 0) {
        this.loadRegionAttractions(this.topRegions[0].regionId);
        this.startAutoSlide();
      }
    });

  }

  //[YJ]點卡片導到景點詳情頁
  goToAttractionDetail(id: number): void {
    this.router.navigate(['/attractions/detail', id]);
  }
  //[YJ] 取得景點主圖完整 URL
  getAttractionImage(a: Attraction): string {
    return a.mainImage
      ? `https://localhost:7285${a.mainImage}`
      : 'assets/img/b1.jpg';
  }
  //[YJ] 首頁景點卡片按讚
  toggleHomeLike(e: Event, spot: Attraction): void {
    e.stopPropagation();
    this.attractionSvc.toggleLike(spot.attractionId).subscribe(res => {
      spot.likeCount = res.likeCount;
      spot.isLiked = res.liked; // ← 直接用 API 回傳的值，liked=false 就取消
    });
  }

  // 當使用者點擊提示詞時
  selectSuggestion(word: string) {
    this.searchControl.setValue(word); // 把字填入框框
    this.showSuggestions = false;     // 關閉選單
    this.onSearch();                  // 直接執行搜尋
  }
  onSearch() {
    const term = this.searchControl.value;

    // 1. 基本檢查：如果沒打字就不執行
    if (!term || term.trim() === '') {
      this.searchBridge.pushData([]); // 清空搜尋結果
      this.showSuggestions = false;   // 關閉提示選單
      return;
    }

    // 2. 呼叫抓取「完整資料 (含圖片)」的 API
    this.searchService.getSearchResults(term).subscribe({
      next: (results) => {
        // 3. 把貨推給橋樑，讓子元件 <app-global-search> 收到並顯示
        this.searchBridge.pushData(results);

        // 4. 關閉提示詞選單（因為已經搜尋了）
        this.showSuggestions = false;

        // 5. 定格跳轉：如果搜得到東西，就捲動到結果區
        if (results.length > 0) {
          this.scrollToResults();
        }
      },
      error: (err) => {
        console.error('搜尋失敗：', err);
        // 這裡可以加個通知說「伺服器斷線中」之類的
      }
    });
  }

  // 輔助方法：捲動到結果區塊
  private scrollToResults() {
    setTimeout(() => {
      // 這裡的 650 是預估 Hero 區的高度，你可以根據畫面調整
      // 或者使用 element.scrollIntoView 更加精準
      window.scrollTo({
        top: 650,
        behavior: 'smooth'
      });
    }, 200);
  }

  favoriteList = [
    {
      id: 1,
      title: '九份山城散策',
      region: '新北',
      category: '景點',
      description: '老街、茶樓與山海景色一次收集。',
      imageUrl: 'https://images.unsplash.com/photo-1526481280695-3c4691dbbb72?auto=format&fit=crop&w=900&q=80'
    },
    {
      id: 2,
      title: '花蓮海岸線一日遊',
      region: '花蓮',
      category: '行程',
      description: '山海交會的經典路線，適合放鬆出走。',
      imageUrl: 'https://images.unsplash.com/photo-1506744038136-46273834b3fb?auto=format&fit=crop&w=900&q=80'
    },
    {
      id: 3,
      title: '台南古城美食路線',
      region: '台南',
      category: '文章',
      description: '從牛肉湯到老屋巷弄，一次吃遍。',
      imageUrl: 'https://images.unsplash.com/photo-1517248135467-4c7edcad34c4?auto=format&fit=crop&w=900&q=80'
    }
  ];

  articleList = [
    {
      id: 1,
      tag: '旅遊攻略',
      title: '2026 台北兩天一夜這樣玩',
      summary: '整合景點、美食與交通建議，適合第一次來台北的旅人。',
      date: '2026-03-25',
      imageUrl: 'https://images.unsplash.com/photo-1513407030348-c983a97b98d8?auto=format&fit=crop&w=900&q=80'
    },
    {
      id: 2,
      tag: '季節主題',
      title: '春季賞花景點推薦',
      summary: '精選北中南東值得安排的花季路線。',
      date: '2026-03-21',
      imageUrl: 'https://images.unsplash.com/photo-1462275646964-a0e3386b89fa?auto=format&fit=crop&w=900&q=80'
    },
    {
      id: 3,
      tag: '在地體驗',
      title: '夜市美食探索地圖',
      summary: '從經典小吃到新派創意點心，吃貨必收。',
      date: '2026-03-18',
      imageUrl: 'https://images.unsplash.com/photo-1555396273-367ea4eb4db5?auto=format&fit=crop&w=900&q=80'
    }
  ];

  eventList = [
    {
      id: 1,
      title: '阿里山日出鐵道體驗',
      location: '嘉義',
      description: '搭配林鐵與山林景觀的熱門活動。',
      score: 4.8,
      price: 1680,
      imageUrl: 'https://images.unsplash.com/photo-1500530855697-b586d89ba3ee?auto=format&fit=crop&w=900&q=80'
    },
    {
      id: 2,
      title: '宜蘭溫泉放鬆小旅行',
      location: '宜蘭',
      description: '泡湯、美食、慢旅行一次滿足。',
      score: 4.7,
      price: 1380,
      imageUrl: 'https://images.unsplash.com/photo-1519046904884-53103b34b206?auto=format&fit=crop&w=900&q=80'
    },
    {
      id: 3,
      title: '墾丁海上活動體驗',
      location: '屏東',
      description: '適合喜歡陽光與海洋的玩家。',
      score: 4.9,
      price: 2200,
      imageUrl: 'https://images.unsplash.com/photo-1507525428034-b723cf961d3e?auto=format&fit=crop&w=900&q=80'
    },
    {
      id: 4,
      title: '九族文化村親子遊',
      location: '南投',
      description: '適合家庭客群的熱門體驗。',
      score: 4.6,
      price: 980,
      imageUrl: 'https://images.unsplash.com/photo-1527631746610-bca00a040d60?auto=format&fit=crop&w=900&q=80'
    }
  ];



  packageList = [
    {
      id: 1,
      title: '花東三日精華套裝',
      days: '3 天 2 夜',
      description: '結合海岸、公路、美食與住宿安排的輕鬆路線。',
      tags: ['含住宿', '親子適合', '熱門路線'],
      score: 4.8,
      price: 6990,
      imageUrl: 'https://images.unsplash.com/photo-1476514525535-07fb3b4ae5f1?auto=format&fit=crop&w=900&q=80'
    },
    {
      id: 2,
      title: '阿里山日月潭經典套裝',
      days: '2 天 1 夜',
      description: '山景與湖景雙主題的人氣經典組合。',
      tags: ['自然景觀', '交通便利', '熱銷'],
      score: 4.7,
      price: 4880,
      imageUrl: 'https://images.unsplash.com/photo-1500534314209-a25ddb2bd429?auto=format&fit=crop&w=900&q=80'
    },
    {
      id: 3,
      title: '台南高雄南部文化行',
      days: '3 天 2 夜',
      description: '老城、美食、港灣風景一次打包。',
      tags: ['美食', '古蹟', '城市漫遊'],
      score: 4.6,
      price: 5590,
      imageUrl: 'https://images.unsplash.com/photo-1482192505345-5655af888cc4?auto=format&fit=crop&w=900&q=80'
    }
  ];

  serviceList = [
    { icon: '✈️', title: '交通資訊', desc: '機場、鐵路、高鐵與轉乘資訊' },
    { icon: '🧾', title: '旅遊須知', desc: '簽證、退稅、常見問題整理' },
    { icon: '🗺️', title: '旅遊地圖', desc: '快速查看地區與熱門景點' },
    { icon: '📶', title: '旅遊便利', desc: 'Wi-Fi、支付與旅客服務資訊' }
  ];
}
