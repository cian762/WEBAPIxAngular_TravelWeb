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

  cartItems: CartItem[] = [];
  isLoading: boolean = true;
  totalAmount: number = 0;

  // [YJ] 勾選功能
  selectedIds = new Set<number>();

  toggleSelect(cartId: number): void {
    if (this.selectedIds.has(cartId)) {
      this.selectedIds.delete(cartId);
    } else {
      this.selectedIds.add(cartId);
    }
  }

  toggleSelectAll(): void {
    if (this.isAllSelected) {
      this.selectedIds.clear();
    } else {
      this.cartItems.forEach(item => this.selectedIds.add(item.cartId));
    }
  }

  get isAllSelected(): boolean {
    return this.cartItems.length > 0 &&
      this.cartItems.every(item => this.selectedIds.has(item.cartId));
  }

  get selectedTotal(): number {
    return this.cartItems
      .filter(item => this.selectedIds.has(item.cartId))
      .reduce((sum, item) => sum + item.price * item.quantity, 0);
  }

  get selectedCount(): number {
    return this.selectedIds.size;
  }
  // [YJ] 勾選功能 end

  ngOnInit(): void {
    this.fetchCartData();
  }

  // 1. 取得購物車資料 (會員→API / 遊客→Local)
  fetchCartData() {
    this.isLoading = true;

    this.cartService.getCart().subscribe({
      next: (data: CartItem[]) => {
        this.cartItems = data;
        this.selectedIds = new Set(data.map((i: CartItem) => i.cartId)); // 預設全選
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
  ItemSwitch(cartIds: number, productCode: string) {
    this.cartService.removeItems([cartIds], [productCode]).subscribe({
      next: () => {
        this.fetchCartData();
      },
      error: (err: any) => console.error('更換票種時移除舊項目失敗', err)
    });
  }

  // 4. 更新數量
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

  // 5. 前往下單（只送勾選的）
  goToOrder() {
    if (this.selectedIds.size === 0) {
      Swal.fire({
        icon: 'warning',
        title: '請至少勾選一項商品',
        timer: 2000,
        showConfirmButton: false
      });
      return;
    }

    const checkoutPayload = {
      directBuyItems: this.cartItems
        .filter(item => this.selectedIds.has(item.cartId))
        .map(item => ({
          productCode: item.productCode,
          productName: item.productName,
          quantity: item.quantity,
          ticketCategoryId: item.ticketCategoryId,
          price: item.price,
          cartId: item.cartId,
          coverImage: item.coverImage,
          targetId: item.targetId
        }))
    };
    console.log('準備帶走的資料:', checkoutPayload);
    this.router.navigate(['/order'], { state: { data: checkoutPayload } });
  }

  // 6. 編輯：導回景點詳情頁售票區
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

  // 7. 計算總價
  calculateTotal() {
    this.totalAmount = this.cartItems.reduce(
      (sum: number, item: CartItem) => sum + (item.price * item.quantity), 0
    );
  }
}
