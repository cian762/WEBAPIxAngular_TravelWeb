import { OrderDetailDto, CreateOrderDto } from './../../models/orderMd.model';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { OrderService } from '../../services/OrderService';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CreateShoppingCart } from '../../services/create-shopping-cart';
import Swal from 'sweetalert2';



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
    // 1. 嘗試從 history.state 抓取資料 (包含商品資料與來源網址)
    const stateData = history.state.data;
    this.checkoutInfo = stateData; // 假設你原本的變數叫 checkoutInfo

    // 2. 檢查資料是否存在
    if (!this.checkoutInfo || !this.checkoutInfo.directBuyItems || this.checkoutInfo.directBuyItems.length === 0) {
      console.warn('遺失結帳資訊，準備導回...');
      this.handleMissingData(stateData?.fromUrl);
      return; // 阻斷後續的 loadOrderPreview，避免報錯
    }

    // 3. 資料正常，執行初始化
    this.initForm();
    this.loadOrderPreview();
  }

  // 提取出來的錯誤處理方法
  private handleMissingData(fromUrl?: string) {
    // 如果有來源網址就回來源，沒有就回首頁
    const fallbackUrl = fromUrl || '/';

    Swal.fire({
      title: '結帳資訊逾時',
      text: '抱歉，系統無法取得您的行程資料，請回到商品頁重新選擇。',
      icon: 'warning',
      confirmButtonText: '回到商品頁',
      confirmButtonColor: '#0d6efd',
      allowOutsideClick: false
    }).then((result) => {
      if (result.isConfirmed) {
        // 使用 navigateByUrl 導回精確的商品詳情頁
        this.router.navigateByUrl(fallbackUrl);
      }
    });
  }


  initForm() {
    this.orderForm = this.fb.group({
      contactName: ['', Validators.required],
      contactEmail: ['', [Validators.required, Validators.email]],
      contactPhone: ['', [Validators.required, Validators.pattern('^09[0-9]{8}$')]],
      customerNote: [''],
      agreeTerms: [false, Validators.requiredTrue]
    });
  }

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
        Swal.fire({
          icon: "error",
          title: '無法取得結帳資訊，請確認商品狀態',
        });
      }
    });
  }

  /** 3. 正式結帳並詢問是否支付 */
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
        // 訂單已經在資料庫成立了！停止 Loading 狀態以顯示彈窗
        this.isLoading = false;

        // --- 1. 處理購物車清理 (非同步，不需要等它完成才彈窗) ---
        const cartIdsToRemove = items
          .map((i: any) => i.cartId)
          .filter((id: number) => id > 0);

        if (cartIdsToRemove.length > 0) {
          this.cartService.removeItems(cartIdsToRemove, []).subscribe({
            next: () => console.log('購物車項目已清理'),
            error: (err) => console.error('購物車清理失敗', err)
          });
        }

        // --- 2. 彈出 Swal 詢問視窗 ---
        Swal.fire({
          title: '訂單成立成功！',
          html: `訂單編號：<b>${res.orderId}</b><br>是否立即前往綠界進行支付？`,
          icon: 'success',
          showConfirmButton: true, // 顯示「立即付款」
          showDenyButton: true,    // 顯示「稍後付款」
          confirmButtonText: '立即付款',
          denyButtonText: '稍後再付',
          confirmButtonColor: '#0d6efd',
          denyButtonColor: '#6c757d',
          allowOutsideClick: false // 防止點擊外面關閉，確保流程明確
        }).then((result) => {
          if (result.isConfirmed) {
            // 選「立即付款」：執行原本的跳轉綠界表單邏輯
            this.handlePaymentRedirect(res);
          } else if (result.isDenied) {
            // 雖然沒付錢，但訂單成立了，我們也可以帶一個狀態回去
            this.router.navigate(['/'], {
              state: { orderCreated: true, orderId: res.orderId }
            });
          }
        });
      },
      error: (err) => {
        this.isLoading = false;
        Swal.fire({
          title: '建立訂單失敗',
          text: err.error?.message || '伺服器發生錯誤，請稍後再試',
          icon: 'error'
        });
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
      // 如果金額為 0 直接成功，帶上 state 跳轉
      this.router.navigate(['/'], {
        state: { paymentSuccess: true, orderId: res.orderId }
      });
    }
  }
}



