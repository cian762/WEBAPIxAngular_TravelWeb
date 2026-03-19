import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Attraction, AttractionType } from './attraction.models';

export interface AttractionProduct {
  productId: number;
  productCode: string;
  title: string;
  price: number | null;
  maxPurchaseQuantity: number | null;
  status: string;
  ticketTypeCode: number | null;
  ticketTypeName: string | null;
}

export interface StockResult {
  productCode: string;
  remainingStock: number;
}

@Injectable({ providedIn: 'root' })
export class AttractionService {
  private apiUrl = 'https://localhost:7276/api';

  constructor(private http: HttpClient) { }

  // 取得所有景點
  getAttractions(params?: {
    regionId?: number;
    typeId?: number;
    keyword?: string;
    pageSize?: number;
  }): Observable<Attraction[]> {
    let p = new HttpParams();
    if (params?.regionId) p = p.set('regionId', params.regionId);
    if (params?.typeId) p = p.set('typeId', params.typeId);
    if (params?.keyword) p = p.set('keyword', params.keyword);
    return this.http.get<Attraction[]>(`${this.apiUrl}/Attraction`, { params: p })
      .pipe(catchError(() => of([])));
  }

  // 取得單一景點
  getAttractionById(id: number): Observable<Attraction | null> {
    return this.http.get<Attraction>(`${this.apiUrl}/Attraction/${id}`)
      .pipe(catchError(() => of(null)));
  }

  // 取得景點分類
  getAttractionTypes(): Observable<AttractionType[]> {
    return this.http.get<AttractionType[]>(`${this.apiUrl}/AttractionType`)
      .pipe(catchError(() => of([])));
  }

  // 按讚
  toggleLike(attractionId: number): Observable<{ likeCount: number; isLiked: boolean }> {
    return this.http.post<{ likeCount: number; isLiked: boolean }>(
      `${this.apiUrl}/Attraction/${attractionId}/like`, {}
    ).pipe(catchError(() => of({ likeCount: 0, isLiked: false })));
  }

  // 依類型取得景點
  getAttractionsByType(typeId: number): Observable<Attraction[]> {
    return this.http.get<Attraction[]>(`${this.apiUrl}/Attraction/bytype/${typeId}`)
      .pipe(catchError(() => of([])));
  }

  // ── 票務相關 ──────────────────────────────────────────

  // 取得某景點所有上架票種
  getProductsByAttraction(attractionId: number): Observable<AttractionProduct[]> {
    return this.http.get<AttractionProduct[]>(
      `${this.apiUrl}/AttractionProduct/byattraction/${attractionId}`
    ).pipe(catchError(() => of([])));
  }

  // 取得單一票種庫存（以 StockInRecords.remaining_stock 加總）
  getStock(productCode: string): Observable<StockResult> {
    return this.http.get<StockResult>(
      `${this.apiUrl}/AttractionProduct/stock/${productCode}`
    ).pipe(catchError(() => of({ productCode, remainingStock: 0 })));
  }
}
