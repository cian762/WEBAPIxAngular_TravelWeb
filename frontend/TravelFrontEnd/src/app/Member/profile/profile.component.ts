import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit {
  activeTab: string = 'account';

  private authService = inject(AuthService);
  private router = inject(Router);

  // ⚠️ 請確保這是您後端 API 的主機網址，用來串接大頭貼的相對路徑
  private backendHost = 'https://localhost:7276';

  userProfile: any = {
    avatarUrl: 'assets/default-avatar.png', // 👈 預設給這張，就不會閃白底
    coverUrl: '',
    accountInfo: {},
    memberInfo: {},
    followingList: [],
    blackList: [],
    complaints: []
  };

  ngOnInit(): void {
    this.authService.getMyProfile().subscribe({
      next: (data) => {
        this.userProfile.memberCode = data.memberCode;
        this.userProfile.memberId = data.memberId;
        this.userProfile.name = data.name;

        if (data.avatarUrl && data.avatarUrl.trim() !== '') {

          // 如果您的後端已經把完整的 Cloudinary 網址存進資料庫，直接拿來用！
          // 例如：data.avatarUrl 會是 "https://res.cloudinary.com/..."
          this.userProfile.avatarUrl = data.avatarUrl;

        } else {
          // 如果資料庫沒圖片，維持預設圖 (您那張很讚的黑人問號圖)
          this.userProfile.avatarUrl = 'assets/default-avatar.png';
        }
        // 🔥 將封面設為空值，不要預設圖片
        this.userProfile.coverUrl = '';

        // 🔥 確保有把後端的資料存入 accountInfo 和 memberInfo 裡面
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

  triggerUpload(type: 'avatar' | 'cover'): void {
    alert(`準備上傳 ${type}`);
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
