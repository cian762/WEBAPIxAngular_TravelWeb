import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CreateShoppingCart } from '../../services/create-shopping-cart';
import { CartItem } from '../../models/creatshopping.model';

@Component({
  selector: 'app-shoppingcart',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './shoppingcart.html',
  styleUrl: './shoppingcart.css',
})

export class Shoppingcart implements OnInit {
  constructor(private http: HttpClient, private cartService: CreateShoppingCart) { }
  cartItems: CartItem[] = []; // 使用強型別介面
  isLoading: boolean = true;
  totalAmount: number = 0;
  memberId: string = 'Briana03'; // 之後從登入狀態拿
  ngOnInit(): void {
    this.fetchCartData();
  }
  // 1. 取得購物車資料
  fetchCartData() {
    this.isLoading = true;

    // 1. 先嘗試從 LocalStorage 拿暫存的購物車
    const localCart = localStorage.getItem('cart');
    let tempItems: CartItem[] = [];

    if (localCart) {
      tempItems = JSON.parse(localCart);
    }

    // 2. 判斷是否已登入 (目前你寫死 Briana03，正式版可以判斷 memberId 是否有效)
    if (this.memberId) {
      // 如果有登入，去後端拿資料
      this.cartService.getCart(this.memberId).subscribe({
        next: (apiData) => {
          // 【進階邏輯】這裡可以選擇將 localCart 與 apiData 合併
          this.cartItems = apiData;
          this.calculateTotal();
          this.isLoading = false;
        },
        error: (err) => {
          console.error('API 讀取失敗，改用本地資料', err);
          this.cartItems = tempItems; // API 失敗時至少還有本地的
          this.calculateTotal();
          this.isLoading = false;
        }
      });
    } else {
      // 3. 沒登入，直接顯示 LocalStorage 的內容
      this.cartItems = tempItems;
      this.calculateTotal();
      this.isLoading = false;
    }
  }

  // 2. 刪除購物車項目的方法
  // 假設傳入 cartId (單選) 或傳入整個選中的陣列 (多選)
  delectCart(cartId: number) {
    if (!confirm('確定要刪除此商品嗎？')) return;

    // 如果有登入，打 API 刪除
    if (this.memberId) {
      this.cartService.removeItems([cartId], this.memberId).subscribe({
        next: () => this.removeFromUI(cartId)
      });
    } else {
      // 如果沒登入，只刪除 LocalStorage
      this.removeFromUI(cartId);
    }
  }

  // 抽出來共用的 UI 刪除邏輯
  removeFromUI(cartId: number) {
    this.cartItems = this.cartItems.filter(item => item.cartId !== cartId);
    // 同步回 LocalStorage，不然重新整理後東西會跑回來
    localStorage.setItem('cart', JSON.stringify(this.cartItems));
    this.calculateTotal();
  }

  // 3. 封裝計算總價的邏輯
  calculateTotal() {
    this.totalAmount = this.cartItems.reduce((sum, item) => sum + (item.price * item.quantity), 0);
  }

}
