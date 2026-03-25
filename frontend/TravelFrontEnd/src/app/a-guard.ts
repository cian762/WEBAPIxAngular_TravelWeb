import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './Member/services/auth.service';
import { map, take } from 'rxjs';

export const aGuard: CanActivateFn = (route, state) => {
  let authService$ = inject(AuthService);
  const router = inject(Router);

  // 1. 抓取瀏覽器所有的 Cookie
  const allCookies = document.cookie;

  // 2. 檢查字串中是否有 "token=" (假設你的 Cookie 名稱叫 token)

  return authService$.checkAuthStatus().pipe(
    take(1),
    map((res) => {
      console.log('API回傳結果', res);
      if (res) {
        return true;
      } else {
        console.log('沒有cookie，是要登入甚麼拉!!');
        return router.parseUrl('/login');
      }
    })
  )
};
