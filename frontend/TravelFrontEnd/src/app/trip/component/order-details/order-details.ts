import { Component, OnInit } from '@angular/core';
import { OrderService } from '../../services/OrderService';
import { CommonModule, NgClass } from '@angular/common';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-order-details',
  imports: [NgClass, CommonModule],
  templateUrl: './order-details.html',
  styleUrl: './order-details.css',
})
export class OrderDetails implements OnInit {
  orders: any[] = [];
  constructor(private readonly orderService: OrderService) { }
  ngOnInit(): void {
    this.loadOrders();
  }
  loadOrders() {
    this.orderService.getMemberOrders().subscribe(res => {
      // 擴充一個 isExpanded 屬性用來控制 UI
      this.orders = res.map(o => ({ ...o, isExpanded: false, details: null }));
    });
  }

  // 展開或收合詳情
  toggleDetail(order: any) {
    order.isExpanded = !order.isExpanded;

    // 如果是第一次展開，且還沒抓過詳情，就去後端補資料
    if (order.isExpanded && !order.details) {
      this.orderService.getOrderDetail(order.orderId).subscribe(detail => {
        order.details = detail;
      });
    }
  }

  // 處理重新支付
  onRepay(orderId: number) {
    this.orderService.repayOrder(orderId).subscribe(res => {
      const paymentWindow = window.open('', '_self');
      paymentWindow?.document.write(res.paymentForm);
    });
  }
  // 處理取消訂單
  onCancel(orderId: number) {
    const orderNo = `#ORD-${orderId.toString().padStart(6, '0')}`;

    // 1. 彈出確認視窗
    Swal.fire({
      title: '確定要取消訂單嗎？',
      text: `訂單編號：${orderNo}，取消後將無法恢復！`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#d33', // 紅色按鈕代表危險操作
      cancelButtonColor: '#6c757d',
      confirmButtonText: '確定取消',
      cancelButtonText: '再想想',
      reverseButtons: true // 習慣上將「確定」放在右邊
    }).then((result) => {

      // 2. 使用者點擊「確定取消」
      if (result.isConfirmed) {

        // 顯示讀取中狀態 (避免重複點擊)
        Swal.showLoading();

        this.orderService.cancelOrder(orderId).subscribe({
          next: (res) => {
            // 3. 成功處理
            // 手動更新前端資料狀態，讓畫面按鈕自動切換
            const targetOrder = this.orders.find(o => o.orderId === orderId);
            if (targetOrder) {
              targetOrder.orderStatus = '已取消';
            }

            Swal.fire({
              title: '已取消！',
              text: `訂單 ${orderNo} 已成功取消。`,
              icon: 'success',
              timer: 1500,
              showConfirmButton: false
            });
          },
          error: (err) => {
            // 4. 失敗處理
            console.error('取消失敗', err);
            Swal.fire({
              title: '取消失敗',
              text: '伺服器目前無法處理您的請求，請稍後再試。',
              icon: 'error',
              confirmButtonText: '了解'
            });
          }
        });
      }
    });
  }

  getStatusBadge(status: string) {
    switch (status) {
      case '待處理': return 'bg-warning text-dark';
      case '已處理': return 'bg-success';
      case '已取消': return 'bg-danger';
      default: return 'bg-secondary';
    }
  }


}
