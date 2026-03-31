import { HttpClient } from '@angular/common/http';
import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { CreateShoppingCart } from '../../services/create-shopping-cart';
import { CartItem } from '../../models/creatshopping.model';
import { AttractionService } from '../../../Components/attractions/attraction.service';
import { forkJoin } from 'rxjs';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-shoppingcart',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './shoppingcart.html',
  styleUrl: './shoppingcart.css',
})
export class Shoppingcart implements OnInit {
  private cartService = inject(CreateShoppingCart);
  private http = inject(HttpClient);
  private router = inject(Router);
  private attractionSvc = inject(AttractionService);

  cartItems: CartItem[] = []; // 使用強型別介面
  isLoading: boolean = true;
  totalAmount: number = 0;


  ngOnInit(): void {
    this.fetchCartData();
  }

  // 1. 取得購物車資料 (會員→API / 遊客→Local)
  fetchCartData() {
    this.isLoading = true;

    // 💡 直接呼叫 Service，後端已經把所有 ID (TargetId) 準備好了
    this.cartService.getCart().subscribe({
      next: (data: CartItem[]) => {
        this.cartItems = data;
        this.calculateTotal();
        this.isLoading = false;
      },
      error: (err: any) => {
        console.error('讀取購物車失敗', err);
        this.isLoading = false;
        Swal.fire('錯誤', '無法取得購物車資料', 'error');
      }
    });
  }

  // 2. 刪除購物車項目
  delectCart(cartIds: number, productCode: string) {
    if (!confirm('確定要刪除此商品嗎？')) return;

    // 同樣交給 Service 處理判斷邏輯
    // 傳入 cartId (後端用) 和 productCode (遊客模式用)
    this.cartService.removeItems([cartIds], [productCode]).subscribe({
      next: () => {
        // 刪除成功後，畫面直接重新拉取一次資料即可 (或者手動 filter)
        this.fetchCartData();
      },
      error: (err: any) => Swal.fire('刪除失敗: ' + err.message)
    });
  }
  // 3. 更換票種直接先刪除
  ItemSwitch(cartIds: number, productCode: string) {
    if (!confirm('確定要更換此商品票種嗎？')) return;

    // 同樣交給 Service 處理判斷邏輯
    // 傳入 cartId (後端用) 和 productCode (遊客模式用)
    this.cartService.removeItems([cartIds], [productCode]).subscribe({
      next: () => {
        // 刪除成功後，畫面直接重新拉取一次資料即可 (或者手動 filter)
        this.fetchCartData();
      },
      error: (err: any) => Swal.fire('刪除失敗: ' + err.message)
    });
  }

  // 3. 更新數量 (如果你畫面上有 + / - 按鈕)
  changeQuantity(item: CartItem, newQty: number) {
    if (newQty < 1) return;

    const oldQty = item.quantity;
    item.quantity = newQty;
    this.calculateTotal();

    this.cartService.updateQuantity(item.cartId, newQty, item.productCode).subscribe({
      next: (res: any) => {
        // 💡 這裡很關鍵：如果後端有回傳「最終確定的數量」或「更新後的價格」
        // 我們更新該 item，確保萬一庫存不足時，數字會跳回正確的值
        if (res && res.confirmedQuantity) {
          item.quantity = res.confirmedQuantity;
          this.calculateTotal();
        }
      },
      error: (err) => {
        item.quantity = oldQty;
        this.calculateTotal();
        Swal.fire('更新失敗', '庫存不足或系統錯誤', 'error');
      }
    });
  }
  //購物車打包給訂單用
  goToOrder() {
    // 假設你的購物車資料存在 this.cartItems
    // 這裡要把資料打包，格式要跟你的 OrderComponent 接收的一致
    const checkoutPayload = {
      directBuyItems: this.cartItems.map(item => ({
        productCode: item.productCode,
        productName: item.productName,
        quantity: item.quantity,
        ticketCategoryId: item.ticketCategoryId,
        price: item.price,
        cartId: item.cartId, // 這是為了之後刪除用的
        coverImage: item.coverImage, // 讓訂單頁能顯示圖片
        targetId: item.targetId
      }))
    };
    console.log('準備帶走的資料:', checkoutPayload);
    // 使用 router.navigate 並透過 state 傳資料
    this.router.navigate(['/order'], { state: { data: checkoutPayload } });
  }

  // 編輯：導回景點詳情頁售票區
  goEdit(item: CartItem) {
    const id = item.targetId;
    const code = item.productCode || '';
    this.ItemSwitch(item.cartId, item.productCode);
    // 1. 景點票券 (TKT-)
    if (code.startsWith('TKT-')) {
      console.log('有進來了', id);
      this.router.navigate(
        ['/attractions/detail', id],
        { queryParams: { tab: 'tickets' } }
      );
    }
    // 2. 套裝行程 (TP)
    else if (code.startsWith('TP')) {
      this.router.navigate(['/trip-detail', id]);
    }
    // 3. 活動體驗 (ACT-)
    else if (code.startsWith('ACT-')) {
      // 💡 這裡路徑請對應你 AppRouting 裡的設定
      this.router.navigate(['/ActivityInfo', id]);
    }
    else {
      // 防呆：如果代碼不匹配，回景點列表
      this.router.navigate(['/attractions']);
    }
  }

  // 4. 計算總價
  calculateTotal() {
    this.totalAmount = this.cartItems.reduce(
      (sum: number, item: CartItem) => sum + (item.price * item.quantity), 0
    );
  }
}
