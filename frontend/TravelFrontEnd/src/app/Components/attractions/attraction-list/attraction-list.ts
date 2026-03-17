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
  attractions: Attraction[] = [];
  types: AttractionType[] = [];
  cityName = '';
  regionIds: number[] = [];
  activeTypeId = 0;
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

  ngOnInit(): void {
    this.route.queryParams.subscribe(p => {
      this.cityName = p['cityName'] || '全部景點';
      this.regionIds = p['regionIds']
        ? String(p['regionIds']).split(',').map(Number)
        : [];
      this.loadTypes();
      this.loadAttractions();
    });
  }

  loadTypes(): void {
    this.svc.getAttractionTypes().subscribe(t => {
      this.types = [{ attractionTypeId: 0, attractionTypeName: 'All' }, ...t];
    });
  }

  loadAttractions(): void {
    this.loading = true;
    this.svc.getAttractions({
      regionId: this.regionIds[0] ?? undefined,
      typeId: this.activeTypeId || undefined,
      keyword: this.keyword || undefined,
    }).subscribe(data => {
      console.log('收到資料:', data);  // ← 加這行
      this.attractions = data;
      this.loading = false;
      this.cdr.detectChanges();   // ← 加這行
    });
  }

  selectType(id: number): void { this.activeTypeId = id; this.loadAttractions(); }
  onSearch(): void { this.loadAttractions(); }
  getTypeIcon(name: string): string { return this.typeIcons[name] ?? '📍'; }

  getMainImage(a: Attraction): string {
    if (a.mainImage) {
      return `https://localhost:7276${a.mainImage}`;
    }
    return 'assets/img/b1.jpg';
  }

  goToDetail(a: Attraction): void {
    this.router.navigate(['/contact/detail', a.attractionId]);
  }

  toggleLike(e: Event, a: Attraction): void {
    e.stopPropagation();
    this.svc.toggleLike(a.attractionId).subscribe(res => {
      a.likeCount = res.likeCount;
      a.isLiked = res.isLiked;
    });
  }
}
