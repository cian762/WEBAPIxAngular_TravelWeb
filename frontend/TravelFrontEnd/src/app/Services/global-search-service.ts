import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class GlobalSearchService {
  private apiUrl = 'https://localhost:7276/api/GlobalSearch';

  constructor(private http: HttpClient) { }

  // 1. 抓取完整搜尋結果 (給首頁大卡片用的：有圖、有 ID、有 Category)
  getSearchResults(q: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}?q=${q}`);
  }

  // 2. 抓取純文字建議 (給搜尋框下拉選單用的：只有 Title 字串陣列)
  // 這支就是你說少的那一支！
  getSuggestions(q: string): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/suggestions?q=${q}`);
  }

}
