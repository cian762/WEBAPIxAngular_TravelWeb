import { Component, OnInit, inject, ViewChild, ElementRef } from '@angular/core'; // 🔥 新增 ViewChild, ElementRef
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit {
  activeTab: string = 'account';

  private authService = inject(AuthService);
  private router = inject(Router);

  // 🔥 取得 HTML 中隱藏的 input 元素
  @ViewChild('avatarInput') avatarInput!: ElementRef<HTMLInputElement>;
  @ViewChild('coverInput') coverInput!: ElementRef<HTMLInputElement>;

  userProfile: any = {
    avatarUrl: 'assets/default-avatar.png',
    coverUrl: '',
    accountInfo: {},
    memberInfo: {},
    followingList: [],
    blackList:[],
    complaints:[]
  };

  ngOnInit(): void {
    this.authService.getMyProfile().subscribe({
      next: (data) => {
        this.userProfile.memberCode = data.memberCode;
        this.userProfile.memberId = data.memberId;
        this.userProfile.name = data.name;

        // 處理大頭貼
        if (data.avatarUrl && data.avatarUrl.trim() !== '') {
          this.userProfile.avatarUrl = data.avatarUrl;
        } else {
          this.userProfile.avatarUrl = 'assets/default-avatar.png';
        }

        // 🔥 處理封面圖片：讀取後端的 backgroundUrl
        if (data.backgroundUrl && data.backgroundUrl.trim() !== '') {
          this.userProfile.coverUrl = data.backgroundUrl;
        } else {
          this.userProfile.coverUrl = '';
        }

        this.userProfile.accountInfo.email = data.email || '未提供信箱';
        this.userProfile.accountInfo.phone = data.phone || '未提供電話';

        this.userProfile.memberInfo.gender = data.gender;
        this.userProfile.memberInfo.birthDate = data.birthDate || '未提供生日';
        this.userProfile.memberInfo.status = data.status;
      },
      error: (err) => {
        alert('無法取得會員資料，請重新登入');
        localStorage.removeItem('isLoggedIn');
        this.authService.authState$.next(false);
        this.router.navigate(['/login']);
      }
    });
  }

  switchTab(tabName: string): void {
    this.activeTab = tabName;
  }

  // 🔥 1. 觸發隱藏的 input 點擊事件，打開檔案總管
  triggerUpload(type: 'avatar' | 'cover'): void {
    if (type === 'avatar') {
      this.avatarInput.nativeElement.click();
    } else if (type === 'cover') {
      this.coverInput.nativeElement.click();
    }
  }

  // 🔥 2. 當使用者選好圖片後觸發此函式，發送給後端
  onFileSelected(event: Event, type: 'avatar' | 'cover'): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      const formData = new FormData();

      // ⚠️ 屬性名稱必須跟後端 C# 的 MemberProfileUpdateDto 一模一樣
      if (type === 'avatar') {
        formData.append('AvatarFile', file);
      } else {
        formData.append('BackgroundFile', file);
      }

      // 呼叫 API 更新資料
      this.authService.updateProfile(formData).subscribe({
        next: (res: any) => {
          // 上傳成功，將後端回傳的 Cloudinary 網址更新到畫面上
          if (type === 'avatar' && res.avatarUrl) {
            this.userProfile.avatarUrl = res.avatarUrl;
          } else if (type === 'cover' && res.backgroundUrl) {
            this.userProfile.coverUrl = res.backgroundUrl;
          }
          alert('圖片更新成功！');
        },
        error: (err) => {
          console.error(err);
          alert('圖片更新失敗，請稍後再試。');
        }
      });

      // 清空 input，確保下次選同一張圖也能觸發 change 事件
      input.value = '';
    }
  }

  onLogout(): void {
    if(confirm('確定要登出嗎？')) {
      this.authService.logout().subscribe({
        next: () => {
          this.router.navigate(['/login']);
        },
        error: (err) => {
          this.router.navigate(['/login']);
        }
      });
    }
  }
}
