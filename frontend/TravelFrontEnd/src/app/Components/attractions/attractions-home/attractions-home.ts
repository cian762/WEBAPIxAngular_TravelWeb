import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { RegionGroup, CityCard } from '../attraction.models';

@Component({
  selector: 'app-attractions-home',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './attractions-home.html',
  styleUrls: ['./attractions-home.css']
})
export class AttractionsHomeComponent implements OnInit, OnDestroy {

  // ── Hero 輪播 ──────────────────────────────────────────
  currentSlide = 0;
  private slideTimer: any;

  heroSlides: string[] = [
    'assets/img/attraction/輪播01.jpg',
    'assets/img/attraction/輪播02.jpg',
    'assets/img/attraction/輪播03.jpg',
    'assets/img/attraction/輪播04.jpg',
    'assets/img/attraction/輪播05.jpg',
    'assets/img/attraction/輪播06.jpg',
  ];

  ngOnInit(): void { this.startAutoPlay(); }
  ngOnDestroy(): void { clearInterval(this.slideTimer); }

  private startAutoPlay(): void {
    this.slideTimer = setInterval(() => {
      this.currentSlide = (this.currentSlide + 1) % this.heroSlides.length;
    }, 4500);
  }

  prevSlide(): void {
    clearInterval(this.slideTimer);
    this.currentSlide = (this.currentSlide - 1 + this.heroSlides.length) % this.heroSlides.length;
    this.startAutoPlay();
  }

  nextSlide(): void {
    clearInterval(this.slideTimer);
    this.currentSlide = (this.currentSlide + 1) % this.heroSlides.length;
    this.startAutoPlay();
  }

  goToSlide(i: number): void {
    clearInterval(this.slideTimer);
    this.currentSlide = i;
    this.startAutoPlay();
  }

  // ── 地區 ──────────────────────────────────────────────
  activeRegion = 0;

  regions: RegionGroup[] = [
    {
      label: '北部地區',
      mapImage: '北部.png',
      description: '您可以探訪臺灣最多元的城市魅力——臺北101與故宮博物院、前往歷史典藏文物欣賞；踏走迪化街老街、九份、淡水等充滿懷舊的歷史勝地。',
      cities: [
        { name: '臺北市', regionIds: [10], imageUrl: 'assets/img/attraction/city/臺北市.jpg' },
        { name: '新北市', regionIds: [11], imageUrl: 'assets/img/attraction/city/新北市.jpg' },
        { name: '基隆市', regionIds: [12], imageUrl: 'assets/img/attraction/city/基隆市.jpg' },
        { name: '宜蘭縣', regionIds: [16], imageUrl: 'assets/img/attraction/city/宜蘭縣.webp' },
        { name: '桃園市', regionIds: [13], imageUrl: 'assets/img/attraction/city/桃園市.jpg' },
        { name: '新竹縣', regionIds: [14], imageUrl: 'assets/img/attraction/city/新竹縣.jpg' },
        { name: '新竹市', regionIds: [15], imageUrl: 'assets/img/attraction/city/新竹市.jpg' },
      ]
    },
    {
      label: '中部地區',
      mapImage: '中部.png',
      description: '日月潭的湖光山色、八卦山、梨山、獅頭山等，都可充分感受臺灣多元文化及豐富自然景觀。苗栗、台中、彰化、南投、雲林皆有精彩可期。',
      cities: [
        { name: '苗栗縣', regionIds: [21], imageUrl: 'assets/img/attraction/city/苗栗縣.jpg' },
        { name: '臺中市', regionIds: [20], imageUrl: 'assets/img/attraction/city/台中市.jpg' },
        { name: '彰化縣', regionIds: [22], imageUrl: 'assets/img/attraction/city/彰化縣.jpg' },
        { name: '南投縣', regionIds: [23], imageUrl: 'assets/img/attraction/city/南投縣.webp' },
        { name: '雲林縣', regionIds: [24], imageUrl: 'assets/img/attraction/city/雲林縣.avif' },
      ]
    },
    {
      label: '南部地區',
      mapImage: '南部.png',
      description: '探訪濃厚的歷史文化：品嚐傳統美食小吃、一睹世界級日出奇景；也可刺激水上活動，近覽太平洋壯闊景觀。',
      cities: [
        { name: '嘉義縣', regionIds: [33], imageUrl: 'assets/img/attraction/city/嘉義縣.webp' },
        { name: '嘉義市', regionIds: [32], imageUrl: 'assets/img/attraction/city/嘉義市.webp' },
        { name: '臺南市', regionIds: [31], imageUrl: 'assets/img/attraction/city/台南市.jpg' },
        { name: '高雄市', regionIds: [30], imageUrl: 'assets/img/attraction/city/高雄市.jpg' },
        { name: '屏東縣', regionIds: [34], imageUrl: 'assets/img/attraction/city/屏東縣.avif' },
      ]
    },
    {
      label: '東部地區',
      mapImage: '東部.png',
      description: '壯觀的太魯閣峽谷、美麗的花東縱谷，以及台東都蘭山、綠島等，是享受大自然洗禮的最佳去處。',
      cities: [
        { name: '花蓮縣', regionIds: [40], imageUrl: 'assets/img/attraction/city/花蓮縣.jpg' },
        { name: '臺東縣', regionIds: [41], imageUrl: 'assets/img/attraction/city/台東縣.jpg' },
      ]
    },
    {
      label: '離島地區',
      mapImage: '離島.png',
      description: '澎湖、金門、馬祖各具獨特的自然景觀和歷史文化，是尋訪純淨海洋與閩南文化的好去處。',
      cities: [
        { name: '澎湖縣', regionIds: [50], imageUrl: 'assets/img/attraction/city/澎湖縣.jpg' },
        { name: '金門縣', regionIds: [51], imageUrl: 'assets/img/attraction/city/金門縣.jpg' },
        { name: '連江縣(馬祖)', regionIds: [52], imageUrl: 'assets/img/attraction/city/連江縣.jpg' },
      ]
    },
  ];

  get currentRegion(): RegionGroup { return this.regions[this.activeRegion]; }

  constructor(private router: Router) { }

  selectRegion(i: number): void { this.activeRegion = i; }

  goToCity(city: CityCard): void {
    this.router.navigate(['/attractions/list'], {
      queryParams: { regionIds: city.regionIds.join(','), cityName: city.name }
    });
  }
}
