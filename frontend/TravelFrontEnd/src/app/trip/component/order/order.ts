import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-order',
  imports: [CommonModule, ReactiveFormsModule, CurrencyPipe, DatePipe],
  templateUrl: './order.html',
  styleUrl: './order.css',
})
export class Order implements OnInit {



  ngOnInit(): void {
    throw new Error('Method not implemented.');
  }

  orderForm = new FormGroup({

  });

}
