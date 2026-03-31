import { Injectable, Injector, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { catchError, switchMap, tap } from 'rxjs/operators';
import { CreateShoppingCart } from '../../trip/services/create-shopping-cart';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7276/api';
  public authState$ = new BehaviorSubject<boolean>(this.isLoggedIn());


  constructor(private injector: Injector) { }

  login(loginData: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/Auth/login`, loginData, { withCredentials: true }).pipe(
      tap((res: any) => {
        localStorage.setItem('isLoggedIn', 'true');
        if (res.userCode) localStorage.setItem('userCode', res.userCode);
        if (res.role) localStorage.setItem('role', res.role);
        this.authState$.next(true);
        const cartService = this.injector.get(CreateShoppingCart);
        cartService.cartCount$.subscribe((data) => { console.log('購物車在登入後刷新', data); });
        cartService.syncLocalCartToDb();
      })
    );
  }

  register(formData: FormData): Observable<any> {
    return this.http.post(`${this.apiUrl}/MemberRegister`, formData);
  }

  getMyProfile(): Observable<any> {
    return this.http.get(`${this.apiUrl}/MemberProfile/me`, { withCredentials: true });
  }


  updateProfile(formData: FormData) {
    const apiUrl = 'https://localhost:7276/api/MemberProfile/me';
    return this.http.put(apiUrl, formData, { withCredentials: true });
  }

  submitComplaint(complaintData: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/Complaint/submit`, complaintData, { withCredentials: true });
  }

  logout(): Observable<any> {
    return this.http.post(`${this.apiUrl}/Auth/logout`, {}, { withCredentials: true }).pipe(
      tap(() => {
        const cartService = this.injector.get(CreateShoppingCart);
        cartService.clearCart();
        localStorage.removeItem('isLoggedIn');
        localStorage.removeItem('userCode');
        localStorage.removeItem('role');
        this.authState$.next(false);
      })
    )
  }

  isLoggedIn(): boolean {
    return localStorage.getItem('isLoggedIn') === 'true';
  }
  //20260325李皇毅路由守門員用的方法
  // 2. 強化 API 檢查方法，增加 tap 來同步狀態
  checkAuthStatus(): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/Auth/check-status`, { withCredentials: true }).pipe(
      tap(isLogged => {
        // 關鍵：如果 API 說有登入，就幫 localStorage 補上狀態，防止下次再跳彈窗
        localStorage.setItem('isLoggedIn', isLogged ? 'true' : 'false');
        this.authState$.next(isLogged);
      }),
      catchError(() => {
        localStorage.setItem('isLoggedIn', 'false');
        this.authState$.next(false);
        return of(false);
      })
    );
  }

  sendVerificationCode(email: string): Observable<any> {
    // 💡 注意：我們傳遞的是純字串，所以要在 header 指定 Content-Type
    return this.http.post(`${this.apiUrl}/Auth/send-verification-code`, `"${email}"`, {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  verifyEmailCode(email: string, code: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/Auth/verify-code`, { email, code });
  }

  // 發送忘記密碼驗證信
  forgotPassword(account: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/Auth/forgot-password`, `"${account}"`, {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  // 執行重設密碼
  resetPassword(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/Auth/reset-password`, data);
  }
}
