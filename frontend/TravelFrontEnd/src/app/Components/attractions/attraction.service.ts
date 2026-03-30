import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Attraction, AttractionType } from './attraction.models';
import { environment } from '../../../environments/environment';

export interface AttractionProduct {
  productId: number;
  productCode: string;
  title: string;
  price: number | null;
  originalPrice: number | null;
  validityDays: number | null;
  maxPurchaseQuantity: number | null;
  status: string;
  ticketTypeCode: number | null;
  ticketTypeName: string | null;
  tags: { tagName: string; description: string | null }[];
}

export interface ProductDetailInfo {
  productId: number;
  productCode: string;
  title: string;
  price: number | null;
  originalPrice: number | null;
  validityDays: number | null;
  maxPurchaseQuantity: number | null;
  ticketTypeName: string | null;
  attractionName: string | null;
  tags: { tagName: string; description: string | null }[];
  detail: {
    contentDetails: string | null;
    notes: string | null;
    usageInstructions: string | null;
    includes: string | null;
    excludes: string | null;
    eligibility: string | null;
    cancelPolicy: string | null;
    validityNote: string | null;   // ← 補上這行
  } | null;
  images: {
    imageId: number;
    imagePath: string;
    caption: string | null;
  }[];
}

export interface StockResult {
  productCode: string;
  remainingStock: number;
}

@Injectable({ providedIn: 'root' })
export class AttractionService {
  baseUrl: string = environment.apiBaseUrl;
  private apiUrl = this.baseUrl;

  constructor(private http: HttpClient) { }

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

  getAttractionById(id: number): Observable<Attraction | null> {
    return this.http.get<Attraction>(`${this.apiUrl}/Attraction/${id}`)
      .pipe(catchError(() => of(null)));
  }

  getAttractionTypes(): Observable<AttractionType[]> {
    return this.http.get<AttractionType[]>(`${this.apiUrl}/AttractionType`)
      .pipe(catchError(() => of([])));
  }

  toggleLike(attractionId: number): Observable<{ likeCount: number; isLiked: boolean }> {
    return this.http.post<{ likeCount: number; isLiked: boolean }>(
      `${this.apiUrl}/Attraction/${attractionId}/like`, {}
    ).pipe(catchError(() => of({ likeCount: 0, isLiked: false })));
  }

  getAttractionsByType(typeId: number): Observable<Attraction[]> {
    return this.http.get<Attraction[]>(`${this.apiUrl}/Attraction/bytype/${typeId}`)
      .pipe(catchError(() => of([])));
  }

  // ── 票務相關 ─────────────────────────────────────────

  getProductsByAttraction(attractionId: number): Observable<AttractionProduct[]> {
    return this.http.get<AttractionProduct[]>(
      `${this.apiUrl}/AttractionProduct/byattraction/${attractionId}`
    ).pipe(catchError(() => of([])));
  }

  getProductDetail(productId: number): Observable<ProductDetailInfo | null> {
    return this.http.get<ProductDetailInfo>(
      `${this.apiUrl}/AttractionProduct/${productId}`
    ).pipe(catchError(() => of(null)));
  }

  getStock(productCode: string): Observable<StockResult> {
    return this.http.get<StockResult>(
      `${this.apiUrl}/AttractionProduct/stock/${productCode}`
    ).pipe(catchError(() => of({ productCode, remainingStock: 0 })));
  }
}
