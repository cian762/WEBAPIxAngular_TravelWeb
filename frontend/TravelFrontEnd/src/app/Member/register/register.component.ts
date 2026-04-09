import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors, FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../services/auth.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent implements OnInit {
  registerForm!: FormGroup;
  errorMessage: string = '';
  isLoading: boolean = false;
  selectedFile: File | null = null;
  imagePreviewUrl: string | null = null;
  isEmailVerified: boolean = false;
  isSendingCode: boolean = false;
  isVerifyingCode: boolean = false;
  countdown: number = 0;
  verificationCodeInput: string = '';

  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  ngOnInit(): void {
    this.registerForm = this.fb.group({
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

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile = file;
      const reader = new FileReader();
      reader.onload = () => {
        this.imagePreviewUrl = reader.result as string;
      };
      reader.readAsDataURL(file);
    }
  }

  // ==========================================
  // 🔥 新增：發送驗證碼邏輯 (含防連點防護)
  // ==========================================
  sendCode(): void {
    const emailControl = this.registerForm.get('email');

    // 1. 防呆：信箱格式不對，不給寄
    if (!emailControl || emailControl.invalid) {

      Swal.fire({
        icon: "warning",
        title: "請先輸入正確格式的 Email！",
      });
      emailControl?.markAsTouched();
      return;
    }

    // 🚨 2. 終極防護：如果還在倒數 60 秒內，或是正在寄信中，絕對不允許執行！
    if (this.countdown > 0 || this.isSendingCode) {
      return;
    }

    this.isSendingCode = true;
    this.errorMessage = '';

    // 3. 呼叫後端 API 寄信
    this.authService.sendVerificationCode(emailControl.value).subscribe({
      next: (res) => {
        this.isSendingCode = false;

        // 🔥 寄信成功！立刻啟動 60 秒倒數計時防連點
        this.startCountdown(60);

        Swal.fire({
          title: res.message || '驗證碼已寄出，請前往信箱查看！',
          icon: "info"
        });

      },
      error: (err) => {
        this.isSendingCode = false;
        this.errorMessage = err.error?.message || '寄信失敗，請稍後再試';

        // 💡 實務技巧：如果寄信失敗 (例如信箱重複)，不要鎖死按鈕，讓他改完可以立刻重試
        this.countdown = 0;
      }
    });
  }

  // ==========================================
  // ⏱️ 倒數計時器核心邏輯
  // ==========================================
  // 宣告一個變數來裝計時器，方便隨時清除
  private countdownInterval: any;

  private startCountdown(seconds: number): void {
    this.countdown = seconds;

    // 如果之前有殘留的計時器在跑，先清掉避免秒數狂跳錯亂
    if (this.countdownInterval) {
      clearInterval(this.countdownInterval);
    }

    // 每 1000 毫秒 (1秒) 執行一次
    this.countdownInterval = setInterval(() => {
      this.countdown--; // 秒數 -1

      // 當倒數歸零時，停止計時器，釋放按鈕
      if (this.countdown <= 0) {
        clearInterval(this.countdownInterval);
        this.countdown = 0;
      }
    }, 1000);
  }

  verifyCode(): void {
    const email = this.registerForm.get('email')?.value;
    if (!this.verificationCodeInput || this.verificationCodeInput.length !== 6) {
      Swal.fire({
        title: '請輸入完整的 6 位數驗證碼！',
        icon: "warning"
      });
      return;
    }

    this.isVerifyingCode = true;
    this.errorMessage = '';

    this.authService.verifyEmailCode(email, this.verificationCodeInput).subscribe({
      next: (res) => {
        this.isVerifyingCode = false;
        this.isEmailVerified = true;
        this.registerForm.get('email')?.disable();

        Swal.fire({
          title: '信箱驗證成功！請繼續填寫下方資料完成註冊。',
          icon: "success"
        });
      },
      error: (err) => {
        this.isVerifyingCode = false;
        this.errorMessage = err.error?.message || '驗證碼錯誤或已過期';
      }
    });
  }

  onSubmit(): void {
    if (!this.isEmailVerified) {
      Swal.fire({
        title: '請先完成 Email 信箱驗證！',
        icon: "warning"
      });

      return;
    }

    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const formData = new FormData();
    const formValues = this.registerForm.getRawValue();

    formData.append('Email', formValues.email);
    formData.append('Password', formValues.password);
    formData.append('Name', formValues.name);
    formData.append('Phone', formValues.phone);

    if (formValues.gender) formData.append('Gender', formValues.gender);
    if (formValues.birthDate) formData.append('BirthDate', formValues.birthDate);

    if (this.selectedFile) {
      formData.append('AvatarFile', this.selectedFile, this.selectedFile.name);
    }

    this.authService.register(formData).subscribe({
      next: (res: any) => {
        this.isLoading = false;
        Swal.fire({
          title: '註冊成功！即將跳轉至登入頁面。',
          icon: "success"
        });
        this.router.navigate(['/login']);
      },
      error: (err: any) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || '註冊失敗，請檢查網路或伺服器狀態。';
        console.error('註冊錯誤', err);
      }
    });
  }

  get f() { return this.registerForm.controls; }
}
