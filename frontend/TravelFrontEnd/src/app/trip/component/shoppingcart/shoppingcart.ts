import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
export interface CartItem {
  cartId: number;
  productCode: string;
  productName: string;
  price: number;
  quantity: number;
  coverImage: string;
}

@Component({
  selector: 'app-shoppingcart',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './shoppingcart.html',
  styleUrl: './shoppingcart.css',
})

export class Shoppingcart implements OnInit {
  private readonly apiUrl = 'https://localhost:7276/api/ShoppingCart'
  constructor(private http: HttpClient) { }
  cartItems: any[] = [];
  isLoading: boolean = true;
  totalAmount: number = 0;
  ngOnInit(): void {
    this.fetchCartData();
  }
  fetchCartData() {
    this.isLoading = true;

    // 這裡就是你提到的參數 Briana03
    // 之後可以改成從登入資訊動態取得
    const memberId = 'Briana03';

    // 使用字串插值把 memberId 接在 URL 後面
    // 最終會發送：https://localhost:7276/api/ShoppingCart/Briana03
    this.http.get<any[]>(`${this.apiUrl}/${memberId}`).subscribe({
      next: (data) => {
        this.cartItems = data;


        // 既然你說後端有算，但這支 API 目前回傳的是 IEnumerable (陣列)
        // 我們先在前端把每一項的 (單價 * 數量) 加總起來顯示在右側
        this.totalAmount = this.cartItems.reduce((sum, item) => sum + (item.price * item.quantity), 0);

        this.isLoading = false;
        console.log('成功拿到購物車資料：', data);
      },
      error: (err) => {
        console.error('系統錯誤~購物車無資料', err);
        this.isLoading = false;
      }
    });
  }
  delectCart() {
    let url = 'https://localhost:7276/api/ShoppingCart/remove-items';
    this.http.delete(url).subscribe({

    })

  }

}
