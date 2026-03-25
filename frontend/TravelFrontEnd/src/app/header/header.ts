import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common'; // 🔥 必須引入才能在 HTML 用 *ngIf
import { RouterModule, RouterLink } from '@angular/router';
import { AuthService } from '../Member/services/auth.service'; // 引入服務
import { CreateShoppingCart } from '../trip/services/create-shopping-cart';

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

  // 綁定到畫面的變數
  isLoggedIn: boolean = false;
  userName: string = '';

  ngOnInit(): void {
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
}
