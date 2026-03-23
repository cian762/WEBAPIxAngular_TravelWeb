import { Component, EventEmitter, Input, Output } from '@angular/core';
import { ticketInfoInterface } from '../../Interface/ticketInfoInterface';

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

  quantity: number = 1;

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
}
