import { HttpInterceptorFn } from '@angular/common/http';
import { environment } from '../../../environments/environment';



export const authInterceptor: HttpInterceptorFn = (req, next) => {

  let baseUrl = environment.apiBaseUrl;
  // 檢查是不是打給我們自己後端 API 的請求
  const isApiUrl = req.url.startsWith(baseUrl);

  if (isApiUrl) {
    const authReq = req.clone({
      withCredentials: true
    });
    return next(authReq);
  }

  return next(req);
};
