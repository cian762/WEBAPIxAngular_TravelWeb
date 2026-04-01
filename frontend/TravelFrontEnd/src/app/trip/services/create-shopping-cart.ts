import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { BehaviorSubject, lastValueFrom, Observable, of, tap } from 'rxjs';
import { CartItem } from '../models/creatshopping.model';
import { AuthService } from '../../Member/services/auth.service';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class CreateShoppingCart {

  baseUrl: string = environment.apiBaseUrl;
  private readonly apiUrl = `${this.baseUrl}/ShoppingCart`;
  private authService = inject(AuthService); // 用來判斷登入狀態
  private readonly LOCAL_STORAGE_KEY = 'cart';
  private cartCountSubject = new BehaviorSubject<number>(0);
  cartCount$ = this.cartCountSubject.asObservable();
  constructor(private http: HttpClient) {
    this.refreshCount(); // 初始化時算一次數量
  }
  // --- 1. 取得購物車 ---
  getCart(): Observable<CartItem[]> {
    if (this.authService.isLoggedIn()) {
      // 會員：打 API (後端會自動從 Cookie 抓 ID，不須傳 memberId)
      return this.http.get<CartItem[]>(this.apiUrl, { withCredentials: true }).pipe(
        tap(items => this.cartCountSubject.next(items.length))
      );
    } else {
      // 遊客：回傳本地資料，並用 of 轉成 Observable 保持介面一致
      const items = this.getLocalCartItems();
      this.cartCountSubject.next(items.length);
      return of(items);
    }
  }

  // --- 2. 加入購物車 ---
  addToCart(dto: any): Observable<any> {
    if (this.authService.isLoggedIn()) {
      // 會員：打 API
      return this.http.post(`${this.apiUrl}/add`, dto, { withCredentials: true }).pipe(
        tap(() => this.refreshCount())
      );
    } else {
      // 遊客：存 LocalStorage
      console.log('從私有方法帶出的dto:', dto);
      this.addToLocalCart(dto);
      return of({ message: '已加入本地購物車' });
    }
  }

  // --- 3. 刪除項目 (支援多選) ---
  removeItems(cartIds: number[], productCodes: string[]): Observable<any> {
    if (this.authService.isLoggedIn()) {
      // 會員：打 API (後端只收 cartIds)
      console.log('會員登入');
      return this.http.post(`${this.apiUrl}/remove-items`, { cartIds }, { withCredentials: true }).pipe(
        tap(() => this.refreshCount())
      );
    } else {
      console.log('遊客狀態');
      // 遊客：用 productCode 過濾本地資料
      let cart = this.getLocalCartItems();
      cart = cart.filter(item => !cartIds.includes(item.cartId));
      console.log('有進來這裡', cart);
      this.setLocalCartItems(cart);
      return of({ message: '本地項目已移除' });
    }
  }

  // --- 4. 更新數量 ---
  updateQuantity(cartId: number, quantity: number, productCode: string): Observable<any> {
    if (this.authService.isLoggedIn()) {
      const dto = { cartId, quantity };
      return this.http.patch(`${this.apiUrl}/update-quantity`, dto, { withCredentials: true }).pipe(
        tap(() => this.refreshCount())
      );
    } else {
      const cart = this.getLocalCartItems();
      const item = cart.find(i => i.productCode === productCode);
      if (item) item.quantity = quantity;
      this.setLocalCartItems(cart);
      return of({ message: '本地數量已更新' });
    }
  }

  // --- 5. 同步功能 (登入後由 AuthService 呼叫) ---
  syncLocalCartToDb(): void {
    const items = this.getLocalCartItems();
    if (items.length === 0) return;

    this.http.post(`${this.apiUrl}/sync`, items, { withCredentials: true }).subscribe({
      next: () => {
        localStorage.removeItem(this.LOCAL_STORAGE_KEY);
        this.refreshCount();
      }
    });
  }

  // --- 輔助方法 (Helper Methods) ---

  private getLocalCartItems(): any[] {
    const data = localStorage.getItem(this.LOCAL_STORAGE_KEY);
    return data ? JSON.parse(data) : [];
  }

  private setLocalCartItems(items: any[]): void {
    localStorage.setItem(this.LOCAL_STORAGE_KEY, JSON.stringify(items));
    this.cartCountSubject.next(items.length);
  }

  private addToLocalCart(item: any) {
    const cart = this.getLocalCartItems();

    // 1. 檢查「同產品且同票種」是否已存在
    const existing = cart.find(i =>
      i.productCode === item.productCode &&
      i.ticketCategoryId === item.ticketCategoryId
    );

    if (existing) {
      // 💡 情況 A：完全一樣，直接累加數量
      existing.quantity += item.quantity;
      console.log('更新現有項目數量', existing);
    } else {
      // 💡 情況 B：新產品，或是「同產品但不同票種」
      // 使用當前時間戳記當 ID，保證絕對不會跟別人重複
      const newCartId = Date.now() + Math.floor(Math.random() * 100);

      const newItem = {
        ...item,
        cartId: newCartId, // ✨ 給予區分用的 ID
        coverImage: item.coverImage || item.mainImage,
        // 確保傳進來的資料包含 targetId，否則編輯會噴錯
        targetId: item.targetId
      };

      cart.push(newItem);
      console.log('加入新的購物車項目', newItem);
    }

    // 儲存回 LocalStorage
    this.setLocalCartItems(cart);
  }

  private refreshCount() {
    this.getCart().subscribe();

  }
  clearCart() {
    this.cartCountSubject.next(0);
    // 如果有需要，也可以在這裡清除 localStorage 的購物車暫存
  }

}
