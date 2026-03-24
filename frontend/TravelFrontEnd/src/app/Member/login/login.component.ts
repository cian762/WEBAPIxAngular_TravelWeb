import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms'; // 必須引入才能用 ngModel
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink], // 匯入表單模組
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  // 綁定畫面的資料
  loginData = {
    account: '',
    password: ''
  };

  errorMessage: string = '';
  isLoading: boolean = false;

  // 注入 Service 與 Router
  private authService = inject(AuthService);
  private router = inject(Router);

  // 點擊登入按鈕觸發的方法
  onSubmit(): void {
    if (!this.loginData.account || !this.loginData.password) {
      this.errorMessage = '請輸入帳號與密碼';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.login(this.loginData).subscribe({
      next: (res) => {
        // 登入成功
        this.isLoading = false;

        // ==========================================
        // 🔥 關鍵修正 1：寫入假旗標與基本資訊
        // 雖然真正的 Token 已經安全地存在 Cookie 裡了，
        // 但我們還是要在前端存一個記號，讓 Angular 知道我們「已登入」。
        // 同時可以把後端回傳的 userCode 等不敏感的資料存起來備用。
        // ==========================================
        localStorage.setItem('isLoggedIn', 'true');
        if (res.userCode) localStorage.setItem('userCode', res.userCode);
        if (res.role) localStorage.setItem('role', res.role);

        alert('登入成功！歡迎回來');

        // 跳轉至會員中心頁面
        this.router.navigate(['/profile']);
      },
      error: (err) => {
        // 登入失敗... (維持原本的寫法)
        this.isLoading = false;
        this.errorMessage = err.error?.message || '登入失敗，請檢查網路或伺服器狀態';
      }
    });
  }
}
