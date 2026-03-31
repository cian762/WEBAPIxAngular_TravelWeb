import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-complaint-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './complaint-form.component.html',
  styleUrl: './complaint-form.component.css'
})
export class ComplaintFormComponent implements OnInit {
  complaintForm!: FormGroup;
  isLoading = false;

  memberId = '';
  memberName = '';

  selectedFile: File | null = null;
  imagePreviewUrl: string | null = null;

  subjectOptions = [
    '暴力內容', '色情內容', '仇恨言論',
    '販售管制商品', '自我傷害', '詐騙或不實內容',
    '智慧財產權', '其他'
  ];

  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  ngOnInit(): void {
    this.complaintForm = this.fb.group({
      subject: ['', Validators.required],
      description: ['', [Validators.required, Validators.minLength(10)]],
      replyEmail: ['', [Validators.required, Validators.email]]
    });

    this.authService.getMyProfile().subscribe({
      next: (res) => {
        this.memberId = res.memberId;
        this.memberName = res.name;
        this.complaintForm.patchValue({ replyEmail: res.email });
      },
      error: () => {
        alert('請先登入會員！');
        this.router.navigate(['/login']);
      }
    });
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

  onCancel(): void {
    if (confirm('您輸入的內容將會被清空，確定要返回嗎？')) {
      this.router.navigate(['/profile']);
    }
  }

  onSubmit(): void {
    console.log('表單驗證結果：', this.complaintForm.valid, '錯誤明細：', this.complaintForm.errors);

    if (this.complaintForm.invalid) {
      this.complaintForm.markAllAsTouched();
      return;
    }

    if (!confirm('確定要送出此份申訴表單嗎？送出後將交由專員為您處理。')) {
      return;
    }

    this.isLoading = true;

    const formData = new FormData();
    formData.append('Subject', this.complaintForm.value.subject);
    formData.append('Description', this.complaintForm.value.description);
    formData.append('ReplyEmail', this.complaintForm.value.replyEmail);
    if (this.selectedFile) {
      formData.append('ImageFile', this.selectedFile, this.selectedFile.name);
    }


    this.authService.submitComplaint(formData).subscribe({
      next: (res) => {
        this.isLoading = false;
        alert(`申訴已成功送出！案件編號：${res.complaintId}`);
        this.router.navigate(['/profile']);
      },
      error: (err) => {
        this.isLoading = false;

        const detailError = err.error?.error ? `\n詳細原因：${err.error.error}` : '';
        const msg = err.error?.message || '送出失敗，請稍後再試';

        alert(msg + detailError);

        console.error('🚨 申訴送出失敗的完整錯誤：', err);
      }
    });


  }

  get f() { return this.complaintForm.controls; }
}
