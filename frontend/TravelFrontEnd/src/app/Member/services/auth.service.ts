import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7276/api';

  public authState$ = new BehaviorSubject<boolean>(this.isLoggedIn());

  constructor() { }

  login(loginData: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/Auth/login`, loginData, { withCredentials: true }).pipe(
      // 🔥 必須解開註解！讓 Angular 知道已登入！
      tap((res: any) => {
        localStorage.setItem('isLoggedIn', 'true');
        if (res.userCode) localStorage.setItem('userCode', res.userCode);
        if (res.role) localStorage.setItem('role', res.role);
        this.authState$.next(true);
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
  const apiUrl = 'https://localhost:7276/api/MemberProfile/me';
  return this.http.put(apiUrl, formData, { withCredentials: true });
}

  logout(): Observable<any> {
    return this.http.post(`${this.apiUrl}/Auth/logout`, {}, { withCredentials: true }).pipe(
      // 🔥 必須解開註解！讓 Angular 知道已登出！
      tap(() => {
        localStorage.removeItem('isLoggedIn');
        localStorage.removeItem('userCode');
        localStorage.removeItem('role');
        this.authState$.next(false);
      })
    );
  }

  isLoggedIn(): boolean {
    return localStorage.getItem('isLoggedIn') === 'true';
  }
}
