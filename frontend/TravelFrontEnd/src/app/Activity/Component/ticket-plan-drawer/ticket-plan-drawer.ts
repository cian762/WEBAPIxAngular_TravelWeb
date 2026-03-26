import { Component, EventEmitter, inject, Input, Output } from '@angular/core';
import { ticketInfoInterface } from '../../Interface/ticketInfoInterface';
import { CreateShoppingCart } from '../../../trip/services/create-shopping-cart';
import { Router } from '@angular/router';//連結購物車用


@Component({
  selector: 'app-ticket-plan-drawer',
  imports: [],
  templateUrl: './ticket-plan-drawer.html',
  styleUrl: './ticket-plan-drawer.css',
})
export class TicketPlanDrawer {
  @Input() isOpen: boolean = false;
  @Input() plan: ticketInfoInterface | null = null;

  @Output() closeDrawer = new EventEmitter<void>();

  cartService = inject(CreateShoppingCart);


  quantity: number = 1;

  constructor(private router: Router) { }//連結購物車用

  close() {
    this.quantity = 1;
    this.closeDrawer.emit();
  }

  increaseQty() {
    this.quantity++;
  }

  decreaseQty() {
    if (this.quantity > 1) {
      this.quantity--;
    }
  }

  addToCart() {
    const purchase = {
      productCode: this.plan?.productCode,
      quantity: this.quantity,
      ticketCategoryId: this.plan?.ticketCategoryId,
    };
    console.log(purchase);
    this.cartService.addToCart(purchase).subscribe((data) => {
      console.log(data);
    });
    this.close();
  }
  //連結購物車用
  addToOrder() {
    const orderDetail = {
      directBuyItems: [{
        productCode: this.plan?.productCode,
        quantity: this.quantity,
        ticketCategoryId: this.plan?.ticketCategoryId
      }]
    };
    console.log(orderDetail);
    this.router.navigate(['/order'], { state: { data: orderDetail } });
    this.close();
  }

}
