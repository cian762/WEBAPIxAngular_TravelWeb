import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class SearchBridge {
  // 1. 建立一個水管 (資料流)
  private searchSource = new BehaviorSubject<any[]>([]);

  // 2. 讓首頁可以訂閱這個水管
  currentResults$ = this.searchSource.asObservable();

  constructor() { }

  // 3. 讓搜尋框把抓到的貨塞進水管
  pushData(results: any[]) {
    this.searchSource.next(results);
  }

}
