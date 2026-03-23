import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ProductBasic, ProductItinerary, ProductSchedule } from '../models/tripproduct.model';

@Injectable({
  providedIn: 'root',
})
export class ProductDetailPage {
  constructor(private readonly http: HttpClient) { }
  private readonly apiUrl = 'https://localhost:7276/api/Trip';
  // 1. 取得商品基本資訊
  getBasicInfo(id: number): Observable<ProductBasic> {
    return this.http.get<ProductBasic>(`${this.apiUrl}/${id}/basic`);
  }

  // 2. 取得出發日期與報價
  getSchedules(id: number): Observable<ProductSchedule[]> {
    return this.http.get<ProductSchedule[]>(`${this.apiUrl}/${id}/schedules`);
  }

  // 3. 取得行程細節與多圖
  getItinerary(id: number): Observable<ProductItinerary[]> {
    return this.http.get<ProductItinerary[]>(`${this.apiUrl}/${id}/itinerary`);
  }
}
