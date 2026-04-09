import { Component, OnInit, inject, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { AuthService } from '../services/auth.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterOutlet],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit {
  activeTab: string = 'account';

  private authService = inject(AuthService);
  private router = inject(Router);

  @ViewChild('avatarInput') avatarInput!: ElementRef<HTMLInputElement>;
  @ViewChild('coverInput') coverInput!: ElementRef<HTMLInputElement>;

  userProfile: any = {
    avatarUrl: 'assets/default-avatar.png',
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
          this.userProfile.avatarUrl = data.avatarUrl;
        } else {
          this.userProfile.avatarUrl = 'assets/default-avatar.png';
        }

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

        // 🌟 更新追隨者數量
        this.userProfile.followersCount = data.followersCount || 0;

        // 🌟 接住後端傳來的真實追隨清單！
        this.userProfile.followingList = data.followingList || [];

        // 🌟 接住後端傳來的真實黑名單！
        this.userProfile.blackList = data.blackList || [];

        // 🔥 接住後端傳來的真實申訴紀錄！
        this.userProfile.complaints = data.complaints || [];
      },
      error: (err) => {
        Swal.fire({
          icon: "error",
          title: "無法取得會員資料，請重新登入",
        });
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
    if (type === 'avatar') {
      this.avatarInput.nativeElement.click();
    } else if (type === 'cover') {
      this.coverInput.nativeElement.click();
    }
  }

  onFileSelected(event: Event, type: 'avatar' | 'cover'): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      const formData = new FormData();

      if (type === 'avatar') {
        formData.append('AvatarFile', file);
      } else {
        formData.append('BackgroundFile', file);
      }

      this.authService.updateProfile(formData).subscribe({
        next: (res: any) => {
          if (type === 'avatar' && res.avatarUrl) {
            this.userProfile.avatarUrl = res.avatarUrl;
          } else if (type === 'cover' && res.backgroundUrl) {
            this.userProfile.coverUrl = res.backgroundUrl;
          }
          Swal.fire({
            title: "圖片更新成功！",
            icon: "success",
          });
        },
        error: (err) => {
          console.error(err);
          Swal.fire({
            icon: "error",
            title: "圖片更新失敗，請稍後再試。",
          });
        }
      });

      input.value = '';
    }
  }




  onLogout(): void {

    const swalWithBootstrapButtons = Swal.mixin({
      customClass: {
        confirmButton: "btn btn-success",
        cancelButton: "btn btn-danger me-5"
      },
      buttonsStyling: false
    });

    swalWithBootstrapButtons.fire({
      title: "是否登出帳號?",
      text: "將會跳轉到登入頁面",
      icon: "question",
      showCancelButton: true,
      confirmButtonText: "確認登出",
      cancelButtonText: "取消登出",
      reverseButtons: true
    }).then((result) => {
      if (result.isConfirmed) {
        swalWithBootstrapButtons.fire({
          title: "已登出",
          icon: "success",
          timer: 1000
        });

        this.authService.logout().subscribe({
          next: () => {
            this.router.navigate(['/login']);
          },
          error: (err) => {
            this.router.navigate(['/login']);
          }
        });
      }
    });
  }

  // ==========================================
  // 加入按鈕的點擊事件 (真實打 API 版)
  // ==========================================

  // 👤 取消追隨
  unfollowUser(memberId: string): void {

    const swalWithBootstrapButtons = Swal.mixin({
      customClass: {
        confirmButton: "btn btn-success",
        cancelButton: "btn btn-danger me-5"
      },
      buttonsStyling: false
    });

    swalWithBootstrapButtons.fire({
      title: `確定要取消追隨 ${memberId} 嗎？`,
      icon: "question",
      showCancelButton: true,
      confirmButtonText: "確定取消",
      cancelButtonText: "保留追蹤",
      reverseButtons: true
    }).then((result) => {
      if (result.isConfirmed) {
        this.authService.toggleFollow(memberId).subscribe({
          next: (res: any) => {
            Swal.fire({
              title: res.message,
              icon: "success",
            });

            // 🔥 神奇魔法：從畫面上直接濾掉(刪除)這個人，不用重新打 API 拿整個清單！
            this.userProfile.followingList = this.userProfile.followingList.filter(
              (f: any) => f.memberId !== memberId
            );
          },
          error: (err: any) => {
            Swal.fire({
              title: err.error?.message || '取消追隨失敗，請稍後再試',
              icon: "error",
            });
          }
        });
      }
    });
  }

  // 🚫 解除封鎖
  unblockUser(blockedId: string): void {

    const swalWithBootstrapButtons = Swal.mixin({
      customClass: {
        confirmButton: "btn btn-success",
        cancelButton: "btn btn-danger me-5"
      },
      buttonsStyling: false
    });

    swalWithBootstrapButtons.fire({
      title: `確定要解除對 ${blockedId} 的封鎖嗎？`,
      icon: "question",
      showCancelButton: true,
      confirmButtonText: "解除封鎖",
      cancelButtonText: "維持封鎖",
      reverseButtons: true
    }).then((result) => {

      if (result.isConfirmed) {
        this.authService.toggleBlock(blockedId).subscribe({
          next: (res: any) => {
            Swal.fire({
              title: res.message,
              icon: "success",
            });

            // 🔥 一樣的神奇魔法：從畫面上直接把這個人移出黑名單！
            this.userProfile.blackList = this.userProfile.blackList.filter(
              (b: any) => b.blockedId !== blockedId
            );
          },
          error: (err: any) => {
            Swal.fire({
              title: err.error?.message || '解除封鎖失敗，請稍後再試',
              icon: "error",
            });
          }
        });
      }
    });
  }
}

