import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { SearchBridge } from '../Services/search-bridge';
import { Router } from '@angular/router';

const categoryMap: Record<string, string> = {
  'ArticleA': '文章',
  'ArticleB': '文章',
  'Activity': '活動',
  'Attraction': '景點',
  'Product': '行程商品'
};

@Component({
  selector: 'app-global-search',
  imports: [CommonModule],
  templateUrl: './global-search.html',
  styleUrl: './global-search.css',
})

export class GlobalSearch implements OnInit {
  displayResults: any[] = [];
  constructor(
    private searchBridge: SearchBridge,
    private router: Router
  ) { }
  ngOnInit(): void {
    // 訂閱橋樑，首頁搜尋框一推資料，這裡就會自動更新
    this.searchBridge.currentResults$.subscribe(data => {
      this.displayResults = data.map(item => ({
        ...item,
        categoryName: categoryMap[item.category as keyof typeof categoryMap] || item.category
      }));
    });
  }
  // 跳轉邏輯也搬到這裡，讓原件自己處理點擊
  goToDetail(item: any) {
    const routeMap: any = {
      'ArticleA': '/Board/detail',        // 對應 Board -> detail/:id
      'ArticleB': '/Board/JournalDetail',        // 對應 Board -> detail/:id
      'Activity': '/ActivityInfo',       // 對應 ActivityInfo -> :id
      'Attraction': '/attractions/detail', // 對應 attractions -> detail/:id
      'Product': '/trip-detail'          // 對應 trip-detail/:id
    };
    const basePath = routeMap[item.category]; // 如果你之前改成了 type，這裡記得用 type

    if (basePath && item.id) {
      // Angular navigate 會自動幫你加上斜線：/Board/detail/123
      this.router.navigate([basePath, item.id]);
    } else {
      console.error('找不到對應路由或 ID', item);
    }
  }

}
export enum CategoryType {
  ArticleA = '文章',
  ArticleB = '文章',
  Activity = '活動',
  Attraction = '景點',
  Product = '行程商品'
}

