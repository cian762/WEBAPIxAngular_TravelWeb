import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './Member/services/auth.service';
import { map, take } from 'rxjs';
import Swal from 'sweetalert2';

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
        // 在踢人走之前，先噴一個提示
        const returnUrl = encodeURIComponent(state.url);
        Swal.fire({
          toast: true,
          position: 'top',
          icon: 'warning',
          title: '請先登入以繼續操作',
          showConfirmButton: false,
          timer: 5000
        });

        return router.parseUrl(`/login?returnUrl=${returnUrl}`);
      }
    })
  )
};
