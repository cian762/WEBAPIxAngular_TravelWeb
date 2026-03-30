import { HttpInterceptorFn } from '@angular/common/http';
import { environment } from '../../../environments/environment';



export const authInterceptor: HttpInterceptorFn = (req, next) => {

  let baseUrl = environment.apiBaseUrl;
  // 檢查是不是打給我們自己後端 API 的請求
  const isApiUrl = req.url.startsWith(baseUrl);

  // 如果是打給我們的 API，就強制掛上 withCredentials: true
  if (isApiUrl) {
    const authReq = req.clone({
      withCredentials: true
    });
    // 放行修改過的請求
    return next(authReq);
  }

  // 如果是打給別人的 (例如外部天氣 API、Google Map)，就不干涉
  return next(req);
};
