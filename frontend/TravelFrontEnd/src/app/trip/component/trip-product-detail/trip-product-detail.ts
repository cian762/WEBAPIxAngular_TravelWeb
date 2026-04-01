import { Component, OnInit } from '@angular/core';
import { ProductBasic, ProductItinerary, ProductSchedule } from '../../models/tripproduct.model';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ProductDetailPage } from '../../services/product-detail-page';
import { CommonModule, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CreateShoppingCart } from '../../services/create-shopping-cart';
import Swal from 'sweetalert2';
import { AuthService } from '../../../Member/services/auth.service';
import { concat, from } from 'rxjs';
import { concatMap, toArray } from 'rxjs/operators';


@Component({
  selector: 'app-trip-product-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, DecimalPipe],
  templateUrl: './trip-product-detail.html',
  styleUrl: './trip-product-detail.css',
})
export class TripProductDetail implements OnInit {

  // 定義存放資料的變數
  adultCount: number = 1;
  childCount: number = 0;
  selectedSchedule: ProductSchedule | null = null;
  basicInfo?: ProductBasic;
  schedules: ProductSchedule[] = [];
  itineraries: ProductItinerary[] = [];
  constructor(private route: ActivatedRoute, private tripService: ProductDetailPage, private cartService: CreateShoppingCart, private router: Router, private authService: AuthService) { }
  labelId: number = 0;
  ngOnInit(): void {
    // 1. 從路由取得 ID (假設路由定義為 product/:id)
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.labelId = id;
    if (id) {
      this.loadProductData(id);
    }

  }

  loadProductData(id: number): void {
    // 2. 呼叫基本資訊
    this.tripService.getBasicInfo(id).subscribe(data => {
      this.basicInfo = data;
      console.log('基本資訊:', this.basicInfo);
    });

    // 3. 呼叫出發日期
    this.tripService.getSchedules(id).subscribe(data => {
      this.schedules = data;
      // 設定預設值：如果有過濾後的資料，就選第一筆
      if (this.filteredSchedules.length > 0) {
        this.selectedSchedule = this.filteredSchedules[0];
      } else {
        this.selectedSchedule = null;
      }
    });

    // 4. 呼叫行程細節
    this.tripService.getItinerary(id).subscribe(data => {
      this.itineraries = data;
      console.log('行程細節:', this.itineraries);
    });
  }
  get filteredSchedules() {
    if (!this.schedules) return [];

    const now = new Date();
    // 取得 3 個月後的最後一秒
    const threeMonthsLater = new Date();
    threeMonthsLater.setMonth(now.getMonth() + 3);

    return this.schedules.filter(s => {
      const sDate = new Date(s.startDate);
      // 條件：日期必須 >= 今天，且 <= 三個月後
      return sDate >= now && sDate <= threeMonthsLater;
    });
  }
  // --- 3. 統一打包要傳出的資料 ---
  private getBookingData() {
    if (!this.selectedSchedule || !this.basicInfo) return null;

    return {
      productId: this.basicInfo.tripProductId,
      productName: this.basicInfo.productName,
      scheduleId: this.selectedSchedule.productCode,
      startDate: this.selectedSchedule.startDate,

      // ✅ 關鍵修改：改用陣列結構，並把代號寫死在裡面
      items: [
        {
          ticketType: 2,        // 成人代號給 2
          qty: this.adultCount, // 數量抓大人
          price: this.selectedSchedule.price
        },
        {
          ticketType: 3,        // 小孩代號給 3
          qty: this.childCount, // 數量抓小孩
          price: this.selectedSchedule.price // 如果小孩同價的話
        }
      ],

      // 總數量與總價 (方便前端顯示)
      totalQty: this.adultCount + this.childCount,
      totalPrice: this.selectedSchedule.price * (this.adultCount + this.childCount)
    };
  }
  // 加入購物車
  addToCart(): void {
    const data = this.getBookingData();
    if (!data) return;

    const itemsToProcess = data.items.filter(item => item.qty > 0);

    // 💡 改用 from + concatMap，確保第一筆送完才送第二筆，避免資料庫競爭
    from(itemsToProcess).pipe(
      concatMap(item => {
        const cartItem = {
          productCode: data.scheduleId,
          productName: data.productName,
          price: item.price,
          quantity: item.qty,
          ticketCategoryId: item.ticketType,
          coverImage: this.basicInfo?.coverImage,
          targetId: this.basicInfo?.tripProductId || data.productId
        };
        return this.cartService.addToCart(cartItem);
      }),
      toArray() // 等全部做完
    ).subscribe({
      next: () => Swal.fire(`成功加入購物車！`),
      error: (err) => console.error('加入失敗', err)
    });
  }
  nowBuy(): void {
    const data = this.getBookingData();
    if (!data || !this.selectedSchedule) {
      Swal.fire('提示', '請選擇出發日期', 'warning');
      return;
    }

    const items = data.items.filter(item => item.qty > 0);
    if (items.length === 0) {
      Swal.fire('提示', '請選擇購買人數', 'warning');
      return;
    }

    // --- 改用 Observable 進行權威檢查 ---
    this.authService.checkAuthStatus().subscribe(isLoggedIn => {
      if (!isLoggedIn) {
        // 真的沒登入，才跳彈窗
        Swal.fire({
          title: '請先登入',
          text: '登入後即可完成行程預訂',
          icon: 'info',
          showCancelButton: true,
          confirmButtonText: '前往登入',
          cancelButtonText: '先逛逛',
          confirmButtonColor: '#0d6efd',
        }).then((result) => {
          if (result.isConfirmed) {
            this.router.navigate(['/login'], { queryParams: { returnUrl: this.router.url } });
          }
        });
      } else {
        // 確定已登入，直接執行導向，這時 state 絕對不會丟失！
        const checkoutPayload = {
          productId: this.basicInfo?.tripProductId,
          fromUrl: this.router.url,
          directBuyItems: items.map(item => ({
            productCode: data.scheduleId,
            quantity: item.qty,
            ticketCategoryId: item.ticketType,
          }))
        };

        this.router.navigate(['/order'], { state: { data: checkoutPayload } });
      }
    });
  }
}





