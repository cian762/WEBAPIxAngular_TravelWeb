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
  //測試中先寫死後面回來改
  private readonly memberId = 'Briana03';

  private getParams(): HttpParams {
    return new HttpParams().set('memberId', this.memberId);
  }
  /** 1. 取得結帳預覽 (POST: api/Order/preview?memberId=...) */
  getPreview(dto: CreateOrderDto): Observable<OrderDetailDto> {
    return this.http.post<OrderDetailDto>(`${this.apiUrl}/preview`, dto, {
      params: this.getParams()
    });
  }

  /** 2. 正式建立訂單 (POST: api/Order?memberId=...) */
  createOrder(dto: CreateOrderDto): Observable<any> {
    // 注意：你後端回傳的是包含 PaymentForm 的物件，不是純 OrderDetailDto
    return this.http.post<any>(this.apiUrl, dto, {
      params: this.getParams()
    });
  }

  /** 3. 取得會員所有訂單 (GET: api/Order/my-orders?memberId=...) */
  getMemberOrders(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/my-orders`, {
      params: this.getParams()
    });
  }

  /** 4. 取得單筆訂單詳情 (GET: api/Order/{orderId}?memberId=...) */
  getOrderDetail(orderId: number): Observable<OrderDetailDto> {
    return this.http.get<OrderDetailDto>(`${this.apiUrl}/${orderId}`, {
      params: this.getParams()
    });
  }

  /** 5. 取消訂單 (PUT: api/Order/cancel/{orderId}/{memberId}) */
  // 移除參數中的 memberId: string
  cancelOrder(orderId: number): Observable<any> {
    // 直接使用 class 內部的 this.memberId ('Briana03')
    // 網址會變成：https://localhost:7276/api/Order/cancel/123/Briana03
    return this.http.put(`${this.apiUrl}/cancel/${orderId}/${this.memberId}`, {});
  }
}
