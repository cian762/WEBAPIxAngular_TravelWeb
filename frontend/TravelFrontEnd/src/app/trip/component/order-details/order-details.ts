import { Component, OnInit } from '@angular/core';
import { OrderService } from '../../services/OrderService';
import { CommonModule, NgClass } from '@angular/common';

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

  getStatusBadge(status: string) {
    switch (status) {
      case 'Pending': return 'bg-warning text-dark';
      case 'Active': return 'bg-success';
      case 'Cancelled': return 'bg-danger';
      default: return 'bg-secondary';
    }
  }


}
