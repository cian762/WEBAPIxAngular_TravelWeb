import { OrderDetailDto, CreateOrderDto } from './../../models/orderMd.model';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { OrderService } from '../../services/OrderService';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CreateShoppingCart } from '../../services/create-shopping-cart';



@Component({
  selector: 'app-order',
  imports: [CommonModule, ReactiveFormsModule, CurrencyPipe, DatePipe, RouterModule],
  templateUrl: './order.html',
  styleUrl: './order.css',
})
export class Order implements OnInit {
  checkoutInfo: any;
  orderForm!: FormGroup;
  previewData?: OrderDetailDto;
  isLoading: boolean = false;
  constructor(private orderService: OrderService, private fb: FormBuilder, private route: ActivatedRoute, private router: Router, private cartService: CreateShoppingCart) {
    // 從 Router 的導航狀態中取出資料
    const navigation = this.router.getCurrentNavigation();
    this.checkoutInfo = navigation?.extras.state?.['data'];
  }



  ngOnInit(): void {
    this.initForm();
    if (!this.checkoutInfo && this.router.navigated) {
      console.warn('遺失結帳資訊，準備導回...');
      // 這裡可以選擇導回購物車或首頁
    }
    this.loadOrderPreview();
  }


  initForm() {
    this.orderForm = this.fb.group({
      contactName: ['', Validators.required],
      contactEmail: ['', [Validators.required, Validators.email]],
      contactPhone: ['', [Validators.required, Validators.pattern('^09[0-9]{8}$')]],
      customerNote: ['']
    });
  }
  /** 2. 載入訂單預覽 (RxJS 呼叫) */
  /** 2. 載入訂單預覽 (RxJS 呼叫) */
  loadOrderPreview() {
    this.isLoading = true;

    // 修改點：優先從 this.checkoutInfo 抓取「立即購買」的商品清單
    const directItems = this.checkoutInfo?.directBuyItems || [];

    const previewDto: CreateOrderDto = {
      contactName: '',
      contactEmail: '',
      contactPhone: '',
      customerNote: '',
      directBuyItems: directItems // 直接使用傳過來的陣列
    };
    console.log('檢查傳給後端的 DTO:', previewDto);

    this.orderService.getPreview(previewDto).subscribe({
      next: (data) => {
        this.previewData = data;
        this.isLoading = false;
        console.log('預覽成功:', data);
      },
      error: (err) => {
        this.isLoading = false;
        console.error('預覽失敗:', err);
        alert('無法取得結帳資訊，請確認商品狀態');
      }
    });
  }

  /** 3. 正式結帳並跳轉金流 */
  onCheckout() {
    if (this.orderForm.invalid || this.isLoading) return;

    this.isLoading = true;

    // 取得結帳商品資訊
    const items = this.checkoutInfo?.directBuyItems || [];

    const finalDto: CreateOrderDto = {
      ...this.orderForm.value,
      directBuyItems: items
    };

    this.orderService.createOrder(finalDto).subscribe({
      next: (res) => {
        // --- 關鍵清理邏輯開始 ---

        // 從商品清單中找出哪些是有 cartId 的（大於 0 代表來自購物車）
        const cartIdsToRemove = items
          .map((i: any) => i.cartId)
          .filter((id: number) => id > 0);

        if (cartIdsToRemove.length > 0) {
          // 情況 A：來自購物車，先呼叫 removeItems 刪除，成功後再跳轉金流
          this.cartService.removeItems(cartIdsToRemove, []).subscribe({
            next: () => {
              console.log('購物車項目已清理');
              this.handlePaymentRedirect(res);
            },
            error: (err) => {
              console.error('清理失敗，但仍繼續支付流程', err);
              this.handlePaymentRedirect(res);
            }
          });
        } else {
          // 情況 B：立即購買 (cartId 都是 0)，直接跳轉金流
          this.handlePaymentRedirect(res);
        }

        // --- 關鍵清理邏輯結束 ---
      },
      error: (err) => {
        this.isLoading = false;
        alert('建立訂單失敗：' + (err.error?.message || '伺服器錯誤'));
      }
    });
  }
  /** 4. 輔助方法：執行綠界金流 HTML 表單跳轉 */
  private executePaymentForm(formHtml: string) {
    const div = document.createElement('div');
    div.innerHTML = formHtml;
    document.body.appendChild(div);
    const form = div.querySelector('form');
    if (form) {
      form.submit(); // 自動送出表單，頁面會跳轉到綠界
    }
  }
  // 抽取出來的跳轉方法，讓程式碼更乾淨
  private handlePaymentRedirect(res: any) {
    if (res.paymentForm) {
      this.executePaymentForm(res.paymentForm);
    } else {
      // 如果沒有金流表單（例如金額為 0），可能導向成功頁面
      this.router.navigate(['/order-success']);
    }
  }
}



