import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { AttractionService } from '../attraction.service';
import { Attraction, AttractionType } from '../attraction.models';

@Component({
  selector: 'app-attraction-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './attraction-list.html',
  styleUrls: ['./attraction-list.css']
})
export class AttractionListComponent implements OnInit {

  customTabs = [
    { label: 'All', icon: '🗺️', typeIds: [] },
    { label: '旅遊景點', icon: '📷', typeIds: [1, 6, 7, 8, 9] },
    { label: '溫泉景點', icon: '♨️', typeIds: [9, 11, 13] },
    { label: '藝文展館', icon: '🏛️', typeIds: [4, 2] },
    { label: '夜市老街', icon: '🌙', typeIds: [10, 2, 4] },
    { label: '古蹟寺廟', icon: '⛩️', typeIds: [2, 5] },
    { label: '遊樂區', icon: '🎡', typeIds: [3, 11, 13] },
  ];
  activeTabIdx = 0;
  attractions: Attraction[] = [];
  types: AttractionType[] = [];  // ← 加這兩行在 activeTabIdx = 0; 附近
  switchCustomTab(idx: number): void {
    this.activeTabIdx = idx;
    this.activeTypeId = this.customTabs[idx].typeIds[0] ?? 0;
    this.loadAttractions();
  }


  cityName = '';
  regionIds: number[] = [];
  activeTypeId = 0;
  // 在 activeTypeId = 0; 下面加
  activeTab: 'attractions' | 'activities' | 'itineraries' = 'attractions';
  switchTab(tab: 'attractions' | 'activities' | 'itineraries'): void {
    this.activeTab = tab;
  }
  keyword = '';
  loading = true;

  readonly typeIcons: Record<string, string> = {
    'All': '🗺️', '旅遊景點': '📷', '自然景觀': '🏔️',
    '歷史古蹟': '🏛️', '主題樂園': '🎡', '文化藝術': '🎨',
    '宗教廟宇': '⛩️', '生態體驗': '🌿', '海洋水族': '🐠',
    '山岳步道': '🥾', '溫泉景點': '♨️', '夜市老街': '🌙',
  };

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private svc: AttractionService,
    private cdr: ChangeDetectorRef   // ← 加這個
  ) { }

  isFromTag = false; // 新增這個
  ngOnInit(): void {
    this.route.queryParams.subscribe(p => {
      this.cityName = p['cityName'] || '全部景點';
      this.regionIds = p['regionIds']
        ? String(p['regionIds']).split(',').map(Number)
        : [];

      if (p['typeId']) {
        this.isFromTag = true;  // ← 加這行
        this.activeTypeId = Number(p['typeId']);
        const idx = this.customTabs.findIndex(t =>
          t.typeIds.includes(this.activeTypeId)
        );
        this.activeTabIdx = idx >= 0 ? idx : 0;
      } else {
        this.isFromTag = false; // ← 從縣市點進來
        this.activeTypeId = 0;
        this.activeTabIdx = 0;
      }

      this.loadAttractions();
    });
  }



  loadAttractions(): void {
    this.loading = true;
    const tab = this.customTabs[this.activeTabIdx];
    const typeId = this.activeTypeId || tab.typeIds[0] || undefined;
    this.svc.getAttractions({
      regionId: this.regionIds[0] ?? undefined,
      typeId: typeId || undefined,
      keyword: this.keyword || undefined,
    }).subscribe(data => {
      this.attractions = data;
      this.loading = false;
      this.cdr.detectChanges();
    });
  }

  selectType(id: number, name: string): void {
    this.router.navigate(['/attractions/list'], {
      queryParams: {
        regionIds: this.regionIds.join(','),
        cityName: name,
        typeId: id
      }
    });
  }

  onSearch(): void { this.loadAttractions(); }
  getTypeIcon(name: string): string { return this.typeIcons[name] ?? '📍'; }

  getMainImage(a: Attraction): string {
    if (a.mainImage) {
      return `https://localhost:7285${a.mainImage}`;//port改成7285
    }
    return 'assets/img/b1.jpg';
  }

  goToDetail(a: Attraction): void {
    this.router.navigate(['/attractions/detail', a.attractionId]);
  }

  toggleLike(e: Event, a: Attraction): void {
    e.stopPropagation();
    this.svc.toggleLike(a.attractionId).subscribe(res => {
      a.likeCount = res.likeCount;
      a.isLiked = res.isLiked;
    });
  }
}
