import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators'; // 👈 確保最上方有引入 tap

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private apiUrl = 'https://localhost:7276/api'; // ⚠️ 請確保這跟您後端 Swagger 的 Port 號一致

  constructor() { }

  // ==========================================
  // 1. 登入 API (使用 HttpOnly Cookie)
  // ==========================================
  login(loginData: any): Observable<any> {
    // 加上 withCredentials: true，告訴瀏覽器準備接收並儲存 HttpOnly Cookie
    return this.http.post(`${this.apiUrl}/Auth/login`, loginData, { withCredentials: true });
  }

  // ==========================================
  // 2. 註冊 API (因為包含圖片，傳遞 FormData)
  // ==========================================
  register(formData: FormData): Observable<any> {
    // 註冊不需要 Cookie，直接呼叫
    return this.http.post(`${this.apiUrl}/MemberRegister`, formData);
  }

  // ==========================================
  // 3. 取得自己的個人資料 API (從 Cookie 自動帶 Token)
  // ==========================================
  getMyProfile(): Observable<any> {
    // 只要加上 withCredentials: true，瀏覽器就會自動把 AuthToken 這個 Cookie 帶給後端！
    return this.http.get(`${this.apiUrl}/MemberInfo/MyProfile`, { withCredentials: true });
  }

   // ==========================================
  // 4. 登出 API (呼叫後端清除 Cookie，並清除前端旗標)
  // ==========================================
  logout(): Observable<any> {
    return this.http.post(`${this.apiUrl}/Auth/logout`, {}, { withCredentials: true }).pipe(
      // 🔥 關鍵修正 2：攔截登出成功的瞬間，把 localStorage 清乾淨！
      tap(() => {
        localStorage.removeItem('isLoggedIn');
        localStorage.removeItem('userCode');
        localStorage.removeItem('role');
      })
    );
  }


  // ==========================================
  // 5. 檢查是否登入 (檢查瀏覽器是否存有特定的辨識旗標)
  // ==========================================
  isLoggedIn(): boolean {
    // 因為現在 Token 被鎖在 HttpOnly Cookie 裡面，前端 JS 是「絕對讀不到」的。
    // 這在實務上是一個好問題！通常我們會這樣解：
    // (解法A：前端在登入時多存一個假的 isLoggedIn=true 在 localStorage，用來判斷畫面顯示)
    // (解法B：每次重整網頁都去打一次 /api/MemberInfo/MyProfile，成功就代表有登入，失敗就代表沒登入)
    // 這裡我們先用簡單的解法A，您登入時可以多寫一行 localStorage.setItem('isLoggedIn', 'true');
    return localStorage.getItem('isLoggedIn') === 'true';
  }
}
