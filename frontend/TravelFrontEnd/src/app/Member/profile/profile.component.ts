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

  // 準備一個空物件來接 API 資料
  userProfile: any = {
    accountInfo: {},
    memberInfo: {},
    followingList: [],
    blackList: [],
    complaints: []
  };

  ngOnInit(): void {
    // 載入頁面時，打 API 抓取個人資料
    this.authService.getMyProfile().subscribe({
      next: (data) => {
        // 將後端回傳的資料塞入我們的變數中
        this.userProfile.memberCode = data.memberCode;
        this.userProfile.memberId = data.memberId;
        this.userProfile.name = data.name;
        this.userProfile.avatarUrl = data.avatarUrl || 'assets/default-avatar.png';
        this.userProfile.coverUrl = 'https://images.unsplash.com/photo-1469474968028-56623f02e42e?ixlib=rb-1.2.1&auto=format&fit=crop&w=1000&q=80'; // 封面暫時用假圖

        this.userProfile.accountInfo.email = data.email;
        this.userProfile.accountInfo.phone = data.phone;

        this.userProfile.memberInfo.gender = data.gender;
        this.userProfile.memberInfo.birthDate = data.birthDate;
        this.userProfile.memberInfo.status = data.status;

        // 追隨者與黑名單等，等後端補上對應欄位後即可直接 mapping
      },
      error: (err) => {
        alert('無法取得會員資料，請重新登入');
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

  // 🔥 新增：執行登出
  onLogout(): void {
    if(confirm('確定要登出嗎？')) {
      this.authService.logout().subscribe({
        next: () => {
          alert('已成功登出');
          this.router.navigate(['/login']); // 跳回登入頁
        },
        error: (err) => {
          console.error('登出發生錯誤', err);
          this.router.navigate(['/login']); // 就算出錯也強制導回登入頁
        }
      });
    }
  }
}
