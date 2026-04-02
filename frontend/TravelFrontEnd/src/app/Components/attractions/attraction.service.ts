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
  private apiUrl = 'https://localhost:7276/api';

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



  toggleLike(attractionId: number): Observable<{ likeCount: number; liked: boolean }> {
    return this.http.post<{ likeCount: number; liked: boolean }>(
      `${this.apiUrl}/Attraction/${attractionId}/like`, {}
    ).pipe(catchError(() => of({ likeCount: 0, liked: false })));
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

  // 取得相關票券推薦（售票區下方，依標籤相似度）
  getRelatedTickets(attractionId: number, top = 10): Observable<{
    attractionId: number;
    name: string;
    mainImage: string | null;
    ticketTitle: string | null;
    ticketPrice: number | null;
    originalPrice: number | null;
    ticketTypeName: string | null;
    overlapCount: number;
  }[]> {
    return this.http.get<any[]>(
      `${this.apiUrl}/Attraction/${attractionId}/related-tickets?top=${top}`
    ).pipe(catchError(() => of([])));
  }

  // 用 productCode 查詢景點ID、景點名稱、Tags（供登入後購物車補資料用）
  getProductByCode(productCode: string): Observable<{
    productId: number;
    productCode: string;
    attractionId: number;
    attractionName: string | null;
    tags: string[];
  } | null> {
    return this.http.get<any>(
      `${this.apiUrl}/AttractionProduct/bycode/${productCode}`
    ).pipe(catchError(() => of(null)));
  }

  // 取得附近景點（周邊資訊 Tab 用）
  getNearbyAttractions(attractionId: number, radius = 10, top = 8): Observable<{
    attractionId: number;
    name: string;
    address: string | null;
    mainImage: string | null;
    distanceKm: number;
  }[]> {
    return this.http.get<any[]>(
      `${this.apiUrl}/Attraction/${attractionId}/nearby?radius=${radius}&top=${top}`
    ).pipe(catchError(() => of([])));
  }

  // 切換票券收藏
  toggleFavorite(productId: number): Observable<{ isFavorited: boolean; message: string }> {
    return this.http.post<{ isFavorited: boolean; message: string }>(
      `${this.apiUrl}/AttractionProduct/${productId}/favorite`, {}
    ).pipe(catchError(() => of({ isFavorited: false, message: '操作失敗' })));
  }

  // 取得當前會員的收藏票券 ID 清單
  getMyFavorites(): Observable<number[]> {
    return this.http.get<number[]>(
      `${this.apiUrl}/AttractionProduct/my-favorites`
    ).pipe(catchError(() => of([])));
  }

  // 取得五大頂層地區（北/中/南/東/離島）
  getTopRegions(): Observable<{ regionId: number; regionName: string }[]> {
    return this.http.get<{ regionId: number; regionName: string }[]>(
      `${this.apiUrl}/Attraction/regions`
    ).pipe(catchError(() => of([])));
  }
}
