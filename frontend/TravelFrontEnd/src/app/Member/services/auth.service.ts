import { Injectable, Injector, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { catchError, switchMap, tap } from 'rxjs/operators';
import { CreateShoppingCart } from '../../trip/services/create-shopping-cart';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  baseUrl: string = environment.apiBaseUrl;
  private http = inject(HttpClient);
  private apiUrl = this.baseUrl;
  public authState$ = new BehaviorSubject<boolean>(this.isLoggedIn());


  constructor(private injector: Injector) { }

  login(loginData: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/Auth/login`, loginData, { withCredentials: true }).pipe(
      // 🔥 必須解開註解！讓 Angular 知道已登入！
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
    // 🔥 關鍵修復：這裡的路徑必須對應後端新的 MemberProfileController
    return this.http.get(`${this.apiUrl}/MemberProfile/me`, { withCredentials: true });
  }

  // 在你的 AuthService 裡面加入這個方法
  updateProfile(formData: FormData) {
    // 記得換成你真正的後端網址
    const apiUrl = `${this.baseUrl}/MemberProfile/me`;
    return this.http.put(apiUrl, formData, { withCredentials: true });
  }

  submitComplaint(complaintData: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/Complaint/submit`, complaintData, { withCredentials: true });
  }

  logout(): Observable<any> {
    return this.http.post(`${this.apiUrl}/Auth/logout`, {}, { withCredentials: true }).pipe(
      // 🔥 必須解開註解！讓 Angular 知道已登出！
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
  checkAuthStatus(): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/Auth/check-status`).pipe(
      catchError(() => of(false)) // 如果報錯或沒登入，就回傳 false
    );
  }
}
