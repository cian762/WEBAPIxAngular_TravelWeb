import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { SearchBridge } from '../Services/search-bridge';
import { Router } from '@angular/router';

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
      this.displayResults = data;
    });
  }
  // 跳轉邏輯也搬到這裡，讓原件自己處理點擊
  goToDetail(item: any) {
    const routes: any = {
      'Article': '/Board/details',
      'Activity': '/Activity/info',
      'Attraction': '/Attraction/details',
      'Product': '/trip/detail'
    };
    this.router.navigate([routes[item.category], item.id]);
  }

}


