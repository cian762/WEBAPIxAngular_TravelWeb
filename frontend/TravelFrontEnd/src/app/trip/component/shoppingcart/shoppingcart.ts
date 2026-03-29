import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { forkJoin } from 'rxjs';
import { CreateShoppingCart } from '../../services/create-shopping-cart';
import { CartItem } from '../../models/creatshopping.model';
import { AttractionService } from '../../../Components/attractions/attraction.service';

@Component({
  selector: 'app-shoppingcart',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './shoppingcart.html',
  styleUrl: './shoppingcart.css',
})
export class Shoppingcart implements OnInit {
  cartItems: CartItem[] = []; // 使用強型別介面
  isLoading: boolean = true;
  totalAmount: number = 0;

  constructor(
    private http: HttpClient,
    private cartService: CreateShoppingCart,
    private attractionSvc: AttractionService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.fetchCartData();
  }

  // 1. 取得購物車資料 (會員→API / 遊客→Local)
  fetchCartData() {
    this.isLoading = true;

    // 直接呼叫 Service，它會自動回傳「會員 API 資料」或「遊客 Local 資料」
    this.cartService.getCart().subscribe({
      next: (data: CartItem[]) => {
        // 找出缺少 attractionId 的項目（登入後從 API 來的資料）
        const needsEnrich = data.filter((item: CartItem) => !item.attractionId);

        if (needsEnrich.length === 0) {
          // 全部資料都齊全（遊客模式），直接顯示
          this.cartItems = data;
          this.calculateTotal();
          this.isLoading = false;
        } else {
          // 批次補查缺少的欄位（平行發出，不會一個等一個）
          const queries = needsEnrich.map((item: CartItem) =>
            this.attractionSvc.getProductByCode(item.productCode)
          );
          forkJoin(queries).subscribe((results: any[]) => {
            results.forEach((res: any, idx: number) => {
              if (res) {
                needsEnrich[idx].attractionId   = res.attractionId;
                needsEnrich[idx].attractionName = res.attractionName ?? '';
                needsEnrich[idx].tags           = res.tags ?? [];
              }
            });
            this.cartItems = data;
            this.calculateTotal();
            this.isLoading = false;
          });
        }
      },
      error: (err: any) => {
        console.error('讀取購物車失敗', err);
        this.isLoading = false;
      }
    });
  }

  // 2. 刪除購物車項目
  delectCart(cartId: number, productCode: string) {
    if (!confirm('確定要刪除此商品嗎？')) return;

    // 同樣交給 Service 處理判斷邏輯
    // 傳入 cartId (後端用) 和 productCode (遊客模式用)
    this.cartService.removeItems([cartId], [productCode]).subscribe({
      next: () => {
        // 刪除成功後，畫面直接重新拉取一次資料即可 (或者手動 filter)
        this.fetchCartData();
      },
      error: (err: any) => alert('刪除失敗: ' + err.message)
    });
  }

  // 3. 更新數量 (如果你畫面上有 + / - 按鈕)
  changeQuantity(item: CartItem, newQty: number) {
    if (newQty < 1) return;

    this.cartService.updateQuantity(item.cartId, newQty, item.productCode).subscribe({
      next: () => this.fetchCartData()
    });
  }

  // 編輯：導回景點詳情頁售票區
  goEdit(item: CartItem) {
    if (item.attractionId) {
      this.router.navigate(
        ['/attractions/detail', item.attractionId],
        { queryParams: { tab: 'tickets' } }
      );
    } else {
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
