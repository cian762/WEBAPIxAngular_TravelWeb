import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { lastValueFrom, Observable } from 'rxjs';
import { CartItem } from '../models/creatshopping.model';

@Injectable({
  providedIn: 'root',
})
export class CreateShoppingCart {
  private readonly apiUrl = 'https://localhost:7276/api/ShoppingCart';
  constructor(private http: HttpClient) { }
  // 1. 取得購物車清單
  getCart(memberId: string): Observable<CartItem[]> {
    return this.http.get<CartItem[]>(`${this.apiUrl}/${memberId}`);
  }

  /**
   * 2. 加入購物車 (POST)
   * 對應後端：AddToCartAsync(AddToCartDTO dto)
   */
  addToCart(dto: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/add`, dto);
  }

  /**
   * 3. 刪除指定項目 (DELETE) - 支援單選與多選
   * 對應後端：RemoveItemsAsync(List<int> cartIds, string memberId)
   */
  removeItems(cartIds: number[], memberId: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/remove-items`, {
      body: cartIds, // 這裡傳送 List<int>
      params: new HttpParams().set('memberId', memberId) // 傳送 memberId 參數
    });
  }

  /**
   * 4. 更新數量 (PATCH / PUT)
   * 對應後端：UpdateQuantityAsync(UpdateCartQtyDTO dto, string memberId)
   */
  updateQuantity(cartId: number, quantity: number, memberId: string): Observable<any> {
    const dto = { cartId, quantity };
    return this.http.patch(`${this.apiUrl}/update-quantity`, dto, {
      params: new HttpParams().set('memberId', memberId)
    });
  }

  /**
   * 5. 清空購物車 (DELETE)
   * 對應後端：ClearCartAsync(string memberId)
   */
  clearCart(memberId: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/clear/${memberId}`);
  }
  // 把遊客的東西搬到會員下的方法
  // 🏆 關鍵：同步本地購物車到資料庫
  syncLocalCartToDb() {
    const localData = localStorage.getItem('GuestCart');
    if (!localData) return;

    const items = JSON.parse(localData);
    if (items.length === 0) return;

    // 這裡呼叫後端新增的 [HttpPost("sync")]
    // 記得一定要加 { withCredentials: true }，後端才能從 Cookie 認出你是誰
    return this.http.post(`${this.apiUrl}/sync`, items, { withCredentials: true });
  }
  // * /將商品存入 LocalStorage(未登入時使用)

  addToLocalCart(item: any) {
    // 1. 取得現有的購物車 (若無則給空陣列)
    const localCart = localStorage.getItem('cart');
    let cart: any[] = localCart ? JSON.parse(localCart) : [];

    // 2. 檢查是否已經有相同的商品 (比對 ProductCode)
    const existingItem = cart.find(i => i.productCode === item.productCode);

    if (existingItem) {
      // 若已存在，增加數量
      existingItem.quantity += item.quantity;
    } else {
      // 若不存在，推入新物件 (確保欄位名稱與 CartItem 介面一致)
      cart.push({
        cartId: Date.now(), // 暫時產生一個 ID 供前端渲染使用
        productCode: item.productCode,
        productName: item.productName, // 確保你有傳入名稱
        price: item.price,
        quantity: item.quantity,
        ticketCategoryId: item.ticketCategoryId,
        mainImage: item.mainImage // 如果你有存圖片路徑的話
      });
    }

    // 3. 寫回 LocalStorage
    localStorage.setItem('cart', JSON.stringify(cart));
    console.log('已存入 LocalStorage:', cart);
  }

}
