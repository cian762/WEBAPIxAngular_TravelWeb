import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  // 登入表單資料
  loginData = {
    account: '',
    password: ''
  };

  errorMessage: string = '';
  isLoading: boolean = false;

  // ==========================================
  // 🔥 新增：忘記密碼 Modal 相關變數
  // ==========================================
  showForgotModal: boolean = false;
  forgotAccount: string = '';
  isSendingEmail: boolean = false;

  private authService = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  // 1. 一般登入邏輯 (維持您原本的優良設計)
  onSubmit(): void {
    if (!this.loginData.account || !this.loginData.password) {
      this.errorMessage = '請輸入帳號與密碼';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.login(this.loginData).subscribe({
      next: (res) => {
        this.isLoading = false;

        localStorage.setItem('isLoggedIn', 'true');
        if (res.userCode) localStorage.setItem('userCode', res.userCode);
        if (res.role) localStorage.setItem('role', res.role);

        // 發送廣播告訴 Header 狀態改變 (確保您 auth.service.ts 裡有這行)
        this.authService.authState$.next(true);

        alert('登入成功！歡迎回來');

        // 登入成功後，跳轉回原本想去的頁面，或是預設的 /profile
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') || '/profile';
        this.router.navigateByUrl(returnUrl);
      },
      error: (err) => {
        console.error('登入錯誤詳細資訊:', err);
        this.isLoading = false;
        this.errorMessage = err.error?.message || '登入失敗，請檢查網路或伺服器狀態';
      }
    });
  }

  // ==========================================
  // 🔥 新增：忘記密碼邏輯
  // ==========================================

  // 控制開啟或關閉 Modal 視窗
  toggleForgotModal(show: boolean): void {
    this.showForgotModal = show;
    if (!show) {
      this.forgotAccount = ''; // 關閉時清空輸入框
    }
  }

  // 點擊「發送驗證信」按鈕
  onForgotPassword(): void {
    if (!this.forgotAccount) {
      alert('請輸入您註冊時的信箱或帳號');
      return;
    }

    this.isSendingEmail = true;

    // 呼叫 Service 送出重設密碼信件
    this.authService.forgotPassword(this.forgotAccount).subscribe({
      next: (res: any) => {
        this.isSendingEmail = false;
        alert(res.message || '重設密碼驗證信已寄出，請前往信箱查收！');
        this.toggleForgotModal(false); // 寄送成功後自動關閉 Modal
      },
      error: (err: any) => {
        this.isSendingEmail = false;
        alert(err.error?.message || '發送失敗，請稍後再試');
        console.error('發送忘記密碼信件錯誤:', err);
      }
    });
  }
}
