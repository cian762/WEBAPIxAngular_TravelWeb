import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../services/auth.service';
import Swal from 'sweetalert2';

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

  showForgotModal: boolean = false;
  forgotAccount: string = '';
  isSendingEmail: boolean = false;

  countdownTime: number = 0;
  private countdownInterval: any;

  private authService = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

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

        this.authService.authState$.next(true);

        Swal.fire({
          title: "登入成功！歡迎回來",
          icon: "success",
          draggable: true,
          showConfirmButton: false,
          timer: 1000,
        });

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

  toggleForgotModal(show: boolean): void {
    this.showForgotModal = show;
    if (!show) {
      this.forgotAccount = '';
    }
  }

  onForgotPassword(): void {
    if (this.countdownTime > 0 || this.isSendingEmail) {
      return;
    }

    if (!this.forgotAccount) {
      Swal.fire({
        icon: "error",
        title: "請輸入您註冊時的信箱或帳號",
      });
      return;
    }

    this.isSendingEmail = true;

    this.authService.forgotPassword(this.forgotAccount).subscribe({
      next: (res: any) => {
        this.isSendingEmail = false;

        this.startCountdown(30);
        Swal.fire({
          title: "密碼重設服務",
          text: res.message || '重設密碼驗證信已寄出，請前往信箱查收！',
          icon: "info"
        });
      },
      error: (err: any) => {
        this.isSendingEmail = false;

        Swal.fire({
          icon: "error",
          title: err.error?.message || '發送失敗，請稍後再試',
        });
        console.error('發送忘記密碼信件錯誤:', err);
      }
    });
  }

  private startCountdown(seconds: number): void {
    this.countdownTime = seconds;

    if (this.countdownInterval) {
      clearInterval(this.countdownInterval);
    }

    this.countdownInterval = setInterval(() => {
      this.countdownTime--;

      if (this.countdownTime <= 0) {
        clearInterval(this.countdownInterval);
        this.countdownTime = 0;
      }
    }, 1000);
  }
}
