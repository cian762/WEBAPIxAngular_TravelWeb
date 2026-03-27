import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors, FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  // ⚠️ 必須引入 ReactiveFormsModule
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent implements OnInit {
  registerForm!: FormGroup;
  errorMessage: string = '';
  isLoading: boolean = false;
  selectedFile: File | null = null; // 存放使用者選擇的圖片檔案
  imagePreviewUrl: string | null = null; // 用來預覽大頭貼

   // ==========================================
  // 🔥 新增：信箱驗證相關變數
  // ==========================================
  isEmailVerified: boolean = false; // 是否驗證成功
  isSendingCode: boolean = false;   // 是否正在寄信中
  isVerifyingCode: boolean = false; // 是否正在驗證中
  countdown: number = 0;            // 倒數計時 (秒)
  verificationCodeInput: string = ''; // 綁定使用者輸入的 6 位數
  // ==========================================

  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  ngOnInit(): void {
    this.registerForm = this.fb.group({
      // 🔥 注意：當驗證成功後，我們要把 email 欄位鎖起來 (disabled)，防止他偷改去註冊別人信箱！
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8), Validators.pattern(/^(?=.*[A-Za-z])(?=.*\d).{8,}$/)]],
      confirmPassword: ['', [Validators.required]],
      name: ['', Validators.required],
      phone: ['', Validators.required],
      gender: [''],
      birthDate: ['']
    }, {
      validators: this.passwordMatchValidator
    });
  }

  // 2. 自訂驗證器：檢查兩次密碼是否一致
  passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password')?.value;
    const confirmPassword = control.get('confirmPassword')?.value;
    if (password !== confirmPassword) {
      control.get('confirmPassword')?.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    } else {
      return null;
    }
  }

  // 3. 處理圖片選擇事件 (預覽與存入變數)
  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile = file;

      // 使用 FileReader 來產生圖片預覽 URL
      const reader = new FileReader();
      reader.onload = () => {
        this.imagePreviewUrl = reader.result as string;
      };
      reader.readAsDataURL(file);
    }
  }

  // ==========================================
  // 🔥 新增：發送驗證碼邏輯
  // ==========================================
  sendCode(): void {
    const emailControl = this.registerForm.get('email');
    if (!emailControl || emailControl.invalid) {
      alert('請先輸入正確格式的 Email！');
      emailControl?.markAsTouched();
      return;
    }

    this.isSendingCode = true;
    this.errorMessage = '';

    this.authService.sendVerificationCode(emailControl.value).subscribe({
      next: (res) => {
        this.isSendingCode = false;
        alert(res.message || '驗證碼已寄出，請前往信箱查看！');
        this.startCountdown(); // 啟動 60 秒倒數計時防連點
      },
      error: (err) => {
        this.isSendingCode = false;
        this.errorMessage = err.error?.message || '寄信失敗，請稍後再試';
      }
    });
  }

  // 倒數計時器
  private startCountdown(): void {
    this.countdown = 60;
    const interval = setInterval(() => {
      this.countdown--;
      if (this.countdown <= 0) {
        clearInterval(interval);
      }
    }, 1000);
  }

  // ==========================================
  // 🔥 新增：比對驗證碼邏輯
  // ==========================================
  verifyCode(): void {
    const email = this.registerForm.get('email')?.value;
    if (!this.verificationCodeInput || this.verificationCodeInput.length !== 6) {
      alert('請輸入完整的 6 位數驗證碼！');
      return;
    }

    this.isVerifyingCode = true;
    this.errorMessage = '';

    this.authService.verifyEmailCode(email, this.verificationCodeInput).subscribe({
      next: (res) => {
        this.isVerifyingCode = false;
        this.isEmailVerified = true; // 🎉 驗證成功！

        // 驗證成功後，鎖定 Email 輸入框，防止他偷改
        this.registerForm.get('email')?.disable();
        alert('信箱驗證成功！請繼續填寫下方資料完成註冊。');
      },
      error: (err) => {
        this.isVerifyingCode = false;
        this.errorMessage = err.error?.message || '驗證碼錯誤或已過期';
      }
    });
  }

  // 4. 送出表單
  onSubmit(): void {
// 🚨 終極防線：必須先驗證信箱！
    if (!this.isEmailVerified) {
      alert('請先完成 Email 信箱驗證！');
      return;
    }

    if (this.registerForm.invalid) {
      // 如果表單有錯，把所有欄位標記為已觸碰(touched)來觸發紅字提示
      this.registerForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const formData = new FormData();
    const formValues = this.registerForm.getRawValue(); // 💡 用 getRawValue 才能抓到被 disabled 的 Email 欄位

    formData.append('Email', formValues.email);
    formData.append('Password', formValues.password);
    formData.append('Name', formValues.name);
    formData.append('Phone', formValues.phone);

    // 如果有選性別或生日才 append
    if (formValues.gender) formData.append('Gender', formValues.gender);
    if (formValues.birthDate) formData.append('BirthDate', formValues.birthDate);

    // 如果有選照片才 append
    if (this.selectedFile) {
      formData.append('AvatarFile', this.selectedFile, this.selectedFile.name);
    }

    // 呼叫 Service 送出資料
    this.authService.register(formData).subscribe({
      next: (res: any) => {
        this.isLoading = false;
        alert('註冊成功！即將跳轉至登入頁面。');
        this.router.navigate(['/login']); // 註冊成功跳轉登入
      },
      error: (err: any) => {
        this.isLoading = false;
        // 抓取後端傳來的 Conflict 或 BadRequest 訊息
        this.errorMessage = err.error?.message || '註冊失敗，請檢查網路或伺服器狀態。';
        console.error('註冊錯誤', err);
      }
    });
  }

  // (輔助屬性) 讓 HTML 拿取欄位更簡潔
  get f() { return this.registerForm.controls; }
}
