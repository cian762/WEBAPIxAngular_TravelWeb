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
    Swal.fire({
      title: '確定要刪除嗎？',
      text: "刪除後將無法恢復此商品！",
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#3085d6',
      cancelButtonColor: '#d33',
      confirmButtonText: '是的，刪除它！',
      cancelButtonText: '取消'
    }).then((result) => {
      if (result.isConfirmed) {
        // 顯示載入中
        Swal.showLoading();

        this.cartService.removeItems([cartIds], [productCode]).subscribe({
          next: () => {
            this.fetchCartData();
            Swal.fire('已刪除！', '商品已從購物車移除。', 'success');
          },
          error: (err: any) => {
            Swal.fire('錯誤', '刪除失敗: ' + err.message, 'error');
          }
        });
      }
    });
  }
  // 3. 更換票種直接先刪除
  // 3. 更換票種 (優化版：合併處理邏輯)
  ItemSwitch(cartIds: number, productCode: string) {
    // 這裡建議不要用 confirm，因為 goEdit 已經點擊了，通常代表 user 確定要改
    // 如果一定要提示，建議用輕量一點的提示
    this.cartService.removeItems([cartIds], [productCode]).subscribe({
      next: () => {
        this.fetchCartData();
      },
      error: (err: any) => console.error('更換票種時移除舊項目失敗', err)
    });
  }

  // 3. 更新數量 (如果你畫面上有 + / - 按鈕)
  // 3. 更新數量 (增加錯誤處理的彈窗)
  changeQuantity(item: CartItem, newQty: number) {
    if (newQty < 1) return;

    const oldQty = item.quantity;
    item.quantity = newQty;
    this.calculateTotal();

    this.cartService.updateQuantity(item.cartId, newQty, item.productCode).subscribe({
      next: (res: any) => {
        if (res && res.confirmedQuantity) {
          item.quantity = res.confirmedQuantity;
          this.calculateTotal();
        }
      },
      error: (err) => {
        // 回滾數量
        item.quantity = oldQty;
        this.calculateTotal();
        Swal.fire({
          icon: 'error',
          title: '更新失敗',
          text: err.error?.message || '庫存不足或系統錯誤',
          timer: 2000,
          showConfirmButton: false
        });
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
    Swal.fire({
      title: '重新選擇',
      text: '將為您跳轉至頁面，原選擇將先移除',
      icon: 'info',
      showCancelButton: true
    }).then((result) => {
      if (result.isConfirmed) {
        this.cartService.removeItems([item.cartId], [item.productCode]).subscribe({
          next: () => {
            // 刪除成功後再導航
            const id = item.targetId;
            const code = item.productCode || '';

            if (code.startsWith('TKT-')) {
              this.router.navigate(['/attractions/detail', id], { queryParams: { tab: 'tickets' } });
            } else if (code.startsWith('TP')) {
              this.router.navigate(['/trip-detail', id]);
            } else if (code.startsWith('ACT-')) {
              this.router.navigate(['/ActivityInfo', id]);
            } else {
              this.router.navigate(['/attractions']);
            }
          }
        });
      }
    });
  }
  // 4. 計算總價
  calculateTotal() {
    this.totalAmount = this.cartItems.reduce(
      (sum: number, item: CartItem) => sum + (item.price * item.quantity), 0
    );
  }
}
