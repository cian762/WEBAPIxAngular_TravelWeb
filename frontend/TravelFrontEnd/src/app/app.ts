import { Component, OnInit, signal } from '@angular/core';
import { RouterOutlet, RouterLink, Router, NavigationEnd } from '@angular/router';
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
import { filter } from 'rxjs';
import { CommonModule } from '@angular/common';
import { ToastComponent } from './Itinerary/component/toast-component/toast-component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Header, Footer, Banner, CommonModule, ToastComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  showBanner = true;
  constructor(private router: Router) {
    // 監聽路由變換
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: any) => {
      const currentUrl = event.urlAfterRedirects || event.url;

      // 使用 includes 檢查，只要網址裡面包含這個字眼，就隱藏 Banner
      // 或者是判斷是不是「首頁」 (/)
      //const hideRoutes = ['/tripProduct', '/login', '/member'];
      const hidePath = 'tripProduct';

      // 邏輯：如果目前網址包含 hidePath，就不顯示 (false)
      this.showBanner = !currentUrl.includes(hidePath);

    });
  }
  ngOnInit(): void {
    this.initializeGuestId();
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
