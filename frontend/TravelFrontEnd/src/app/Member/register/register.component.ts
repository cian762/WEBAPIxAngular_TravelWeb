import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors, FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../services/auth.service';

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
  // ==========================================

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
        this.startCountdown();
      },
      error: (err) => {
        this.isSendingCode = false;
        this.errorMessage = err.error?.message || '寄信失敗，請稍後再試';
      }
    });
  }

  private startCountdown(): void {
    this.countdown = 60;
    const interval = setInterval(() => {
      this.countdown--;
      if (this.countdown <= 0) {
        clearInterval(interval);
      }
    }, 1000);
  }

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
        this.isEmailVerified = true;
        this.registerForm.get('email')?.disable();
        alert('信箱驗證成功！請繼續填寫下方資料完成註冊。');
      },
      error: (err) => {
        this.isVerifyingCode = false;
        this.errorMessage = err.error?.message || '驗證碼錯誤或已過期';
      }
    });
  }

  onSubmit(): void {
    if (!this.isEmailVerified) {
      alert('請先完成 Email 信箱驗證！');
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
        alert('註冊成功！即將跳轉至登入頁面。');
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
