import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common'; // 🔥 確保有引入
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-banner',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './banner.html',
  styleUrl: './banner.css'
})
export class Banner implements OnInit {

  private router = inject(Router);

  // 決定是否顯示 Banner 的變數 (預設顯示)
  showBanner: boolean = true;

  // 定義哪些路徑「不要」顯示 Banner
  private hiddenPaths = ['/profile', '/login', '/register'];

  ngOnInit(): void {
    // 1. 初始化時先檢查一次目前網址
    this.checkBannerVisibility(this.router.url);

    // 2. 監聽路由切換事件：每當使用者換頁時，重新檢查要不要顯示
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: any) => {
      this.checkBannerVisibility(event.urlAfterRedirects);
    });
  }

  // 判斷邏輯：如果網址包含在 hiddenPaths 陣列中，就隱藏
  private checkBannerVisibility(url: string): void {
    // 使用 some 檢查，只要網址包含其中一個關鍵字，就會回傳 true
    const shouldHide = this.hiddenPaths.some(path => url.includes(path));
    this.showBanner = !shouldHide;
  }
}
