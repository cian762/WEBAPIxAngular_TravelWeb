import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AttractionService, AttractionProduct, ProductDetailInfo } from '../attraction.service';
import { CreateShoppingCart } from '../../../trip/services/create-shopping-cart';

@Component({
  selector: 'app-ticket-section',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './ticket-section.html',
  styleUrls: ['./ticket-section.css']
})
export class TicketSectionComponent implements OnInit {
  @Input() attractionId!: number;
  @Input() coverImage: string = '';
  @Input() attractionName: string = '';  // ← 景點名稱

  loading = true;
  products: AttractionProduct[] = [];

  // productCode → 剩餘庫存
  stockMap: Record<string, number> = {};
  // productCode → 庫存是否已載入
  stockLoaded: Record<string, boolean> = {};
  // productId → 購買數量
  qtyMap: Record<number, number> = {};

  // Accordion：目前展開的 productId（null = 全部收起）
  expandedId: number | null = null;

  // 方案詳情側邊面板
  drawerOpen = false;
  drawerLoading = false;
  drawerDetail: ProductDetailInfo | null = null;

  constructor(
    private svc: AttractionService,
    private cartService: CreateShoppingCart,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.svc.getProductsByAttraction(this.attractionId).subscribe(list => {
      this.products = list;
      this.loading = false;

      list.forEach(p => {
        this.qtyMap[p.productId] = 1;
        this.svc.getStock(p.productCode).subscribe(s => {
          this.stockMap[p.productCode] = s.remainingStock;
          this.stockLoaded[p.productCode] = true;
        });
      });
    });
  }

  // ── Accordion ────────────────────────────────────────

  toggleExpand(productId: number): void {
    this.expandedId = this.expandedId === productId ? null : productId;
  }

  isExpanded(productId: number): boolean {
    return this.expandedId === productId;
  }

  // ── 方案詳情側邊面板 ──────────────────────────────────

  openDrawer(productId: number): void {
    this.drawerOpen = true;
    this.drawerLoading = true;
    this.drawerDetail = null;
    this.svc.getProductDetail(productId).subscribe(d => {
      this.drawerDetail = d;
      this.drawerLoading = false;
    });
  }

  closeDrawer(): void {
    this.drawerOpen = false;
    this.drawerDetail = null;
  }

  // ── 數量控制 ──────────────────────────────────────────

  incQty(p: AttractionProduct): void {
    const cur = this.qtyMap[p.productId] ?? 1;
    const stock = this.stockMap[p.productCode] ?? 0;
    const max = p.maxPurchaseQuantity ?? stock;
    if (cur >= Math.min(max, stock)) return;
    this.qtyMap[p.productId] = cur + 1;
  }

  decQty(productId: number): void {
    this.qtyMap[productId] = Math.max(1, (this.qtyMap[productId] ?? 1) - 1);
  }

  // ── 庫存狀態 ──────────────────────────────────────────

  isLowStock(productCode: string): boolean {
    return this.stockLoaded[productCode] && (this.stockMap[productCode] ?? 0) < 10 && (this.stockMap[productCode] ?? 0) > 0;
  }

  isSoldOut(productCode: string): boolean {
    return this.stockLoaded[productCode] && (this.stockMap[productCode] ?? 0) <= 0;
  }

  // ── 工具方法 ──────────────────────────────────────────

  /** 將換行分隔的字串轉成陣列（用於 includes/excludes/eligibility） */
  toLines(text: string | null | undefined): string[] {
    if (!text) return [];
    // 同時處理真實換行 \n 和字面上的 \n 字串
    return text
      .replace(/\\n/g, '\n')
      .split('\n')
      .map(s => s.trim())
      .filter(s => s.length > 0);
  }

  /** 計算小計 */
  getSubtotal(p: AttractionProduct): number {
    return (p.price ?? 0) * (this.qtyMap[p.productId] ?? 1);
  }

  addToCart(p: AttractionProduct): void {
    const qty = this.qtyMap[p.productId] ?? 1;
    const dto = {
      productCode: p.productCode,
      productName: p.title,
      price: p.price ?? 0,
      quantity: qty,
      coverImage: this.coverImage,
      ticketCategoryId: p.ticketTypeCode ?? 0,
      targetId: this.attractionId,
      attractionName: this.attractionName,
      tags: p.tags?.map(t => t.tagName) ?? [],
    };
    console.log('加入購物車 dto:', JSON.stringify(dto));
    this.cartService.addToCart(dto).subscribe({
      next: () => alert(`已加入購物車：${p.title} × ${qty}`),
      error: (err: any) => alert('加入購物車失敗：' + err.message),
    });
  }

  nowBuy(p: AttractionProduct): void {
    const qty = this.qtyMap[p.productId] ?? 1;
    const orderDetail = {
      directBuyItems: [{
        productCode: p.productCode,
        quantity: qty,
        ticketCategoryId: p.ticketTypeCode ?? 0,
      }]
    };
    this.router.navigate(['/order'], { state: { data: orderDetail } });
  }
}
