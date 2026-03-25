import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { CreateOrderDto, OrderDetailDto } from '../models/orderMd.model';
import { Observable } from 'rxjs';


@Injectable({
  providedIn: 'root',
})
export class OrderService {
  constructor(private http: HttpClient) { }
  private readonly apiUrl = 'https://localhost:7276/api/Order';

  /** 1. 取得結帳預覽 (POST: api/Order/preview) */
  getPreview(dto: CreateOrderDto): Observable<OrderDetailDto> {
    // 攔截器會自動補上 Cookie，所以這裡只要專注傳 DTO 即可
    return this.http.post<OrderDetailDto>(`${this.apiUrl}/preview`, dto);
  }

  /** 2. 正式建立訂單 (POST: api/Order) */
  createOrder(dto: CreateOrderDto): Observable<any> {
    return this.http.post<any>(this.apiUrl, dto);
  }

  /** 3. 取得會員所有訂單 (GET: api/Order/my-orders) */
  getMemberOrders(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/my-orders`);
  }

  /** 4. 取得單筆訂單詳情 (GET: api/Order/{orderId}) */
  getOrderDetail(orderId: number): Observable<OrderDetailDto> {
    return this.http.get<OrderDetailDto>(`${this.apiUrl}/${orderId}`);
  }

  /** 5. 取消訂單 (PUT: api/Order/cancel/{orderId}) */
  cancelOrder(orderId: number): Observable<any> {
    // ✅ 重點：網址後面的 memberId 也要拿掉，後端改從 Token 抓
    return this.http.put(`${this.apiUrl}/cancel/${orderId}`, {});
  }


}
