import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  // ⚠️ 必須引入 ReactiveFormsModule
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent implements OnInit {
  registerForm!: FormGroup;
  errorMessage: string = '';
  isLoading: boolean = false;
  selectedFile: File | null = null; // 存放使用者選擇的圖片檔案
  imagePreviewUrl: string | null = null; // 用來預覽大頭貼

  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  ngOnInit(): void {
    // 1. 初始化響應式表單 (對應後端 DTO 的驗證規則)
    this.registerForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      // 密碼：必填、至少 8 碼、英數混合 (Regex)
      password: ['', [Validators.required, Validators.minLength(8), Validators.pattern(/^(?=.*[A-Za-z])(?=.*\d).{8,}$/)]],
      confirmPassword: ['', [Validators.required]],
      name: ['', Validators.required],
      phone: ['', Validators.required],
      gender: [''], // 1=男, 2=女
      birthDate: ['']
    }, {
      validators: this.passwordMatchValidator // 綁定自訂的「確認密碼」驗證器
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

  // 4. 送出表單
  onSubmit(): void {
    if (this.registerForm.invalid) {
      // 如果表單有錯，把所有欄位標記為已觸碰(touched)來觸發紅字提示
      this.registerForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    // 準備 FormData (因為包含檔案，不能傳 JSON)
    const formData = new FormData();
    const formValues = this.registerForm.value;

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
