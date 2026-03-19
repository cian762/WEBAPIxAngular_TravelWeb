import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AttractionService, AttractionProduct } from '../attraction.service';

@Component({
  selector: 'app-ticket-section',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './ticket-section.html',
  styleUrls: ['./ticket-section.css']
})
export class TicketSectionComponent implements OnInit {
  @Input() attractionId!: number;

  loading = true;
  products: AttractionProduct[] = [];

  // productCode → 剩餘庫存
  stockMap: Record<string, number> = {};
  // productCode → 庫存是否已載入
  stockLoaded: Record<string, boolean> = {};
  // productId → 購買數量
  qtyMap: Record<number, number> = {};

  constructor(private svc: AttractionService) { }

  ngOnInit(): void {
    this.svc.getProductsByAttraction(this.attractionId).subscribe(list => {
      this.products = list;
      this.loading = false;

      list.forEach(p => {
        // 預設每個票種數量為 1
        this.qtyMap[p.productId] = 1;

        // 取庫存（以 StockInRecords.remaining_stock 加總）
        this.svc.getStock(p.productCode).subscribe(s => {
          this.stockMap[p.productCode] = s.remainingStock;
          this.stockLoaded[p.productCode] = true;
        });
      });
    });
  }

  /** 增加數量，不超過 maxPurchaseQuantity 及剩餘庫存 */
  incQty(p: AttractionProduct): void {
    const cur = this.qtyMap[p.productId] ?? 1;
    const stock = this.stockMap[p.productCode] ?? 0;
    const max = p.maxPurchaseQuantity ?? stock;
    if (cur >= Math.min(max, stock)) return;
    this.qtyMap[p.productId] = cur + 1;
  }

  /** 減少數量，最少 1 */
  decQty(productId: number): void {
    this.qtyMap[productId] = Math.max(1, (this.qtyMap[productId] ?? 1) - 1);
  }

  /** 庫存是否不足（低於 10 張顯示警示） */
  isLowStock(productCode: string): boolean {
    return (this.stockMap[productCode] ?? 0) < 10;
  }

  /** 是否售完 */
  isSoldOut(productCode: string): boolean {
    return this.stockLoaded[productCode] && (this.stockMap[productCode] ?? 0) <= 0;
  }

  addToCart(p: AttractionProduct): void {
    const qty = this.qtyMap[p.productId];
    // TODO: 串接購物車 API
    // payload: { productCode: p.productCode, productId: p.productId, qty }
    alert(`已加入購物車：${p.title} × ${qty}`);
  }
}
