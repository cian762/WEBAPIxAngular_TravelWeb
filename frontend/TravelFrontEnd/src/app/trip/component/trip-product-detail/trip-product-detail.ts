import { Component, OnInit } from '@angular/core';
import { ProductBasic, ProductItinerary, ProductSchedule } from '../../models/tripproduct.model';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { ProductDetailPage } from '../../services/product-detail-page';
import { CommonModule, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-trip-product-detail',
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
  constructor(private route: ActivatedRoute, private tripService: ProductDetailPage) { }
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

    if (data) {
      // 這裡 data 已經包含你剛改好的 items: [{ticketType:2, qty:...}, {ticketType:3, qty:...}]
      console.log('加入購物車參數 (含票種代號):', data);

      // 修正 Alert 顯示：直接用 this.adultCount 和 this.childCount 最快
      const total = this.adultCount + this.childCount;
      alert(`成功加入購物車！\n日期：${data.startDate}\n成人：${this.adultCount}位、兒童：${this.childCount}位`);

      // 未來實作： this.cartService.add(data).subscribe(...);
    }
  }

  // 立即購買
  buyNow(): void {
    const data = this.getBookingData();

    if (data) {
      console.log('立即購買參數 (含票種代號):', data);

      // 這是 Angular 最推薦的跳轉傳參方式：使用 state
      // 這樣在「結帳頁面」就能透過 history.state 拿到這整包 data
      // this.route.navigate(['/checkout'], {
      //   state: {
      //     booking: data
      //   }
      // });

      // 如果你還沒建 checkout 頁面，先用 alert 測試
      // alert('正在前往結帳頁面...');
    }
  }
}



