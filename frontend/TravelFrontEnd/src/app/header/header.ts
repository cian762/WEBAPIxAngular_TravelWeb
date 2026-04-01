import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common'; // 🔥 必須引入才能在 HTML 用 *ngIf
import { RouterModule, RouterLink, ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../Member/services/auth.service'; // 引入服務
import { CreateShoppingCart } from '../trip/services/create-shopping-cart';
import Swal from 'sweetalert2';


@Component({
  selector: 'app-header',
  // ⚠️ 加入 CommonModule，HTML 才能使用 Angular 語法
  imports: [CommonModule, RouterModule, RouterLink],
  templateUrl: './header.html',
  styleUrl: './header.css',
})
export class Header implements OnInit {
  public cartService = inject(CreateShoppingCart);
  private authService = inject(AuthService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  // 綁定到畫面的變數
  isLoggedIn: boolean = false;
  userName: string = '';

  ngOnInit(): void {
    this.route.queryParamMap.subscribe(params => {
      const payStatus = params.get('paySuccess');

      if (payStatus === 'true') {
        // 顯示成功彈窗
        Swal.fire({
          title: '付款成功！',
          text: '感謝您的購買，祝您旅途愉快！',
          icon: 'success',
          confirmButtonColor: '#0d6efd'
        }).then(() => this.clearUrl());

      } else if (payStatus === 'false') {
        // 顯示失敗彈窗
        Swal.fire({
          title: '付款未成功',
          text: '支付程序似乎出了點問題，請檢查您的信用卡資訊或稍後再試。',
          icon: 'error',
          confirmButtonColor: '#dc3545'
        }).then(() => this.clearUrl());
      }
    });

    // 🔥 訂閱(收聽)登入狀態廣播
    this.authService.authState$.subscribe(status => {
      this.isLoggedIn = status;

      // 如果狀態變成 true (已登入)，立刻打 API 抓名字
      if (this.isLoggedIn) {
        this.authService.getMyProfile().subscribe({
          next: (res) => {
            this.userName = res.name; // 把拿到的姓名存進 userName
          },
          error: (err) => {
            // console.error('抓取姓名失敗，可能是 Cookie 失效了', err);
            // // 防呆：如果 API 拒絕，強制登出
            // this.onLogout();
          }
        });
      } else {
        // 如果是登出狀態，把名字清空
        this.userName = '';
      }
    });
  }

  // 登出按鈕觸發的方法
  onLogout(): void {
    if (confirm('確定要登出嗎？')) {
      this.authService.logout().subscribe({
        next: () => {
          // 登出成功後，廣播會自動把 isLoggedIn 變成 false，畫面會瞬間切換！
          console.log('已成功登出');
        }
      });
    }
  }

  private clearUrl() {
    // 將網址後的 ?paySuccess=true... 移除，變成乾淨的 /
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { paySuccess: null, orderId: null },
      queryParamsHandling: 'merge', // 移除指定參數
      replaceUrl: true // 不會在瀏覽器留下這一頁的歷史紀錄
    });
  }




}
