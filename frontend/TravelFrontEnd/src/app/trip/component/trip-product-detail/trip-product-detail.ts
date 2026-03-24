import { Component, OnInit } from '@angular/core';
import { ProductBasic, ProductItinerary, ProductSchedule } from '../../models/tripproduct.model';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { ProductDetailPage } from '../../services/product-detail-page';
import { CommonModule, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CreateShoppingCart } from '../../services/create-shopping-cart';

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
  constructor(private route: ActivatedRoute, private tripService: ProductDetailPage, private cartService: CreateShoppingCart) { }
  ngOnInit(): void {
    // 1. 從路由取得 ID (假設路由定義為 product/:id)
    const id = Number(this.route.snapshot.paramMap.get('id'));

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

    if (!data || !this.selectedSchedule) {
      alert('請選擇出發日期');
      return;
    }

    // 1. 檢查是否有會員 ID (代表已登入)
    const memberId = localStorage.getItem('memberId');

    // 2. 過濾掉數量為 0 的項目 (不論大人小孩，有買才送)
    const itemsToProcess = data.items.filter(item => item.qty > 0);

    if (itemsToProcess.length === 0) {
      alert('請選擇購買人數');
      return;
    }

    // 3. 根據登入狀態執行不同邏輯
    itemsToProcess.forEach(item => {
      const cartItem = {
        memberId: memberId || '', // 沒登入就給空，之後同步再補
        productCode: data.scheduleId,
        productName: data.productName, // LocalStorage 需要名稱來顯示
        price: item.price,
        quantity: item.qty,
        ticketCategoryId: item.ticketType
      };

      if (memberId) {
        // --- 已登入：打 API 存入資料庫 ---
        this.cartService.addToCart(cartItem).subscribe({
          next: () => console.log(`票種 ${item.ticketType} API 存入成功`),
          error: (err) => console.error('API 存入失敗', err)
        });
      } else {
        // --- 未登入：存入 LocalStorage ---
        this.cartService.addToLocalCart(cartItem);
        console.log(`票種 ${item.ticketType} 已暫存至 LocalStorage`);
      }
    });

    alert(`成功加入購物車！\n日期：${data.startDate}\n成人：${this.adultCount}位、兒童：${this.childCount}位`);
  }
  // 立即購買按鈕
  buyNow(): void {
    const memberId = localStorage.getItem('memberId');

    if (!memberId) {
      // 沒登入的話，先存 LocalStorage 再跳轉登入
      this.addToCart();
      alert('請先登入會員以完成訂單');
      // this.router.navigate(['/login'], { queryParams: { returnUrl: '/shopping-cart' } });
    } else {
      // 已登入的話，存 API 後直接跳轉
      this.addToCart();
      // this.router.navigate(['/shopping-cart']);
    }
  }
}



