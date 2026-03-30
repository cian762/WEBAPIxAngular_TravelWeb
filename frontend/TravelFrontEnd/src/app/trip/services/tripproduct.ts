import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { PagedResult, ProductQueryDTO, RegionListDTO, TagListDTO, TripProductDTO } from '../models/tripproduct.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class Tripproduct {
  constructor(private readonly http: HttpClient) { }
  baseUrl: string = environment.apiBaseUrl;
  apiUrl = `${this.baseUrl}/Trip`;
  // 1. 取得初始化資料 (地區與標籤)
  getMetaData(): Observable<{ regions: RegionListDTO[], tags: TagListDTO[] }> {
    return this.http.get<{ regions: RegionListDTO[], tags: TagListDTO[] }>(`${this.apiUrl}/MetaData`);
  }

  // 2. 執行搜尋與分頁
  searchProducts(query: ProductQueryDTO): Observable<PagedResult<TripProductDTO>> {
    let params = new HttpParams();

    // 關鍵：將 DTO 轉為 Http 參數
    if (query.keyword) params = params.set('keyword', query.keyword);
    if (query.regionId) params = params.set('regionId', query.regionId.toString());
    if (query.page) params = params.set('page', query.page.toString());
    if (query.pageSize) params = params.set('pageSize', query.pageSize.toString());

    // 處理多選標籤 (List<int>)
    if (query.tagIds && query.tagIds.length > 0) {
      query.tagIds.forEach(id => {
        params = params.append('tagIds', id.toString());
      });
    }

    return this.http.get<PagedResult<TripProductDTO>>(`${this.apiUrl}/Search`, { params });
  }
}
