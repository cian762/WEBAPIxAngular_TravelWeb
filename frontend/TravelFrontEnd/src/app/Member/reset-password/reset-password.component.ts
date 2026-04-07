import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../services/auth.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink], // 🔥 必須引入這些模組
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.css'
})
export class ResetPasswordComponent implements OnInit {
  resetForm!: FormGroup;
  targetEmail: string = '';
  isLoading: boolean = false;
  errorMessage: string = '';

  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private authService = inject(AuthService);

  ngOnInit(): void {
    // 1. 從網址參數 (Query Params) 抓取 email
    // 例如網址是：http://localhost:4200/reset-password?email=test@gmail.com
    this.route.queryParams.subscribe(params => {
      this.targetEmail = params['email'] || '';

      // 防呆：如果網址沒有帶 email，直接踢回登入頁
      if (!this.targetEmail) {
        Swal.fire({
          icon: "error",
          title: "無效的重設密碼連結！",
        });
        this.router.navigate(['/login']);
      }
    });

    // 2. 建立表單驗證規則 (包含密碼強度與二次確認)
    this.resetForm = this.fb.group({
      code: ['', [Validators.required, Validators.minLength(4), Validators.maxLength(4)]],
      newPassword: ['', [Validators.required, Validators.minLength(8), Validators.pattern(/^(?=.*[A-Za-z])(?=.*\d).{8,}$/)]],
      confirmPassword: ['', [Validators.required]]
    }, {
      validators: this.passwordMatchValidator
    });
  }

  // 自訂驗證器：檢查兩次密碼是否一致
  passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const newPassword = control.get('newPassword')?.value;
    const confirmPassword = control.get('confirmPassword')?.value;
    if (newPassword !== confirmPassword) {
      control.get('confirmPassword')?.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    }
    return null;
  }

  // 取得表單控制項，方便 HTML 呼叫
  get f() { return this.resetForm.controls; }

  // 3. 點擊「確認重設」按鈕
  onSubmit(): void {
    if (this.resetForm.invalid) {
      this.resetForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    // 準備打給後端的資料 (必須跟後端的 ResetPasswordDto 欄位名稱一致)
    const requestData = {
      email: this.targetEmail,
      code: this.resetForm.value.code,
      newPassword: this.resetForm.value.newPassword
    };

    // 呼叫 AuthService 打 API
    this.authService.resetPassword(requestData).subscribe({
      next: (res: any) => {
        this.isLoading = false;
        Swal.fire({
          title: res.message || '密碼重設成功！請使用新密碼登入。',
          icon: "success",
        });
        // 成功後跳轉回登入頁
        this.router.navigate(['/login']);
      },
      error: (err) => {
        this.isLoading = false;
        // 顯示驗證碼錯誤或過期的訊息
        this.errorMessage = err.error?.message || '密碼重設失敗，請確認驗證碼是否正確或已過期。';
        console.error('重設密碼錯誤:', err);
      }
    });
  }
}
