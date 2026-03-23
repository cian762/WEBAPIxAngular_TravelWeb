import { OrderDetailDto, CreateOrderDto } from './../../models/orderMd.model';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { OrderService } from '../../services/OrderService';
import { ActivatedRoute } from '@angular/router';



@Component({
  selector: 'app-order',
  imports: [CommonModule, ReactiveFormsModule, CurrencyPipe, DatePipe],
  templateUrl: './order.html',
  styleUrl: './order.css',
})
export class Order implements OnInit {
  constructor(private orderService: OrderService, private fb: FormBuilder, private route: ActivatedRoute) { }
  orderForm!: FormGroup;
  previewData?: OrderDetailDto;
  isLoading: boolean = false;


  ngOnInit(): void {
    this.initForm();
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
  loadOrderPreview() {
    this.isLoading = true;

    // A. 從網址抓取可能的「立即購買」參數
    const pCode = this.route.snapshot.queryParamMap.get('pCode');
    const qty = Number(this.route.snapshot.queryParamMap.get('qty') || 1);
    const catId = Number(this.route.snapshot.queryParamMap.get('catId') || 1);

    // 1. 這裡只給商品資訊，其他欄位給空字串或 null
    const previewDto: CreateOrderDto = {
      contactName: '',
      contactEmail: '',
      contactPhone: '',
      customerNote: '',
      // 根據有無 pCode 決定分流
      directBuyItems: pCode ? [{
        productCode: pCode,
        quantity: qty,
        ticketCategoryId: catId
      }] : []
    };

    // C. 呼叫 Service (Service 內部已寫死 MemberId)
    this.orderService.getPreview(previewDto).subscribe({
      next: (data) => {
        this.previewData = data;
        this.isLoading = false;
        console.log('預覽成功:', data);
      },
      error: (err) => {
        this.isLoading = false;
        console.error('預覽失敗:', err);
        alert('無法取得結帳資訊，請確認購物車是否有商品');
      }
    });
  }

  /** 3. 正式結帳並跳轉金流 */
  onCheckout() {
    if (this.orderForm.invalid) {
      this.orderForm.markAllAsTouched();
      alert('請填寫正確的聯絡人資訊');
      return;
    }

    // 同樣判斷一次來源
    const pCode = this.route.snapshot.queryParamMap.get('pCode');
    const finalDto: CreateOrderDto = {
      ...this.orderForm.value,
      directBuyItems: pCode ? [{
        productCode: pCode,
        quantity: Number(this.route.snapshot.queryParamMap.get('qty')),
        ticketCategoryId: Number(this.route.snapshot.queryParamMap.get('catId'))
      }] : []
    };

    this.orderService.createOrder(finalDto).subscribe({
      next: (res) => {
        // res 格式：{ message: '...', orderId: 123, paymentForm: '<html>...</html>' }
        if (res.paymentForm) {
          this.executePaymentForm(res.paymentForm);
        }
      },
      error: (err) => {
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
}



