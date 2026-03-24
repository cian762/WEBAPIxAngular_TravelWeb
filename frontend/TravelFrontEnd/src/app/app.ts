import { Component, OnInit, signal } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';
import { TestUse } from "./Components/test-use/test-use";
import { BlogHome } from "./Board/blog-home/blog-home";
import { Header } from "./header/header";
import { Footer } from "./footer/footer";
import { Banner } from "./banner/banner";
import { PostDetail } from "./Board/post-detail/post-detail";
// import { Shoppingcart } from './trip/component/shoppingcart/shoppingcart';
import { Order } from "./trip/component/order/order";
import { Product } from "./trip/component/product/product";
import { TripProductDetail } from "./trip/component/trip-product-detail/trip-product-detail";
import { LoginComponent } from "./Member/login/login.component";

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, TestUse, RouterLink, BlogHome, Header, Footer, Banner, PostDetail, Order, Product, TripProductDetail, LoginComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  ngOnInit(): void {
    // this.initializeGuestId();
  }
  protected readonly title = signal('TravelFrontEnd');
  initializeGuestId() {
    // 1. 檢查是否已經有登入的會員 (假設你存為 memberId)
    const isLogin = !!localStorage.getItem('memberId');
    if (isLogin) return; // 已登入就不需要遊客 ID

    // 2. 檢查是否已經有遊客 ID
    const guestId = localStorage.getItem('guest_id');

    // 3. 如果沒登入也沒遊客 ID，就給一個新的
    if (!guestId) {
      // 生成符合你後端規範的 "GUEST_" 前綴 ID
      const newGuestId = 'GUEST_' + Math.random().toString(36).substring(2, 11) + Date.now();
      localStorage.setItem('guest_id', newGuestId);
      console.log('分配遊客身份：', newGuestId);
    }
  }
}
