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

  // ── 加入購物車 ────────────────────────────────────────

  addToCart(p: AttractionProduct): void {
    const qty = this.qtyMap[p.productId] ?? 1;

    // 組裝符合後端 AddToCartDTO 的 payload
    // MemberId 不用傳，後端從 JWT Token 自動抓
    const payload = {
      productCode: p.productCode,
      quantity: qty,
      ticketCategoryId: p.ticketTypeCode ?? null  // 景點票種代號，可能為 null
    };

    this.cartService.addToCart(payload).subscribe({
      next: () => {
        alert(`✅ 已加入購物車：${p.title} × ${qty}`);
      },
      error: (err) => {
        console.error('加入購物車失敗', err);
        alert('加入購物車失敗，請稍後再試');
      }
    });
  }

  // ── 立即預訂 ──────────────────────────────────────────
  // TODO: 待串接，目前景點票券的立即預訂流程待確認
  nowBuy(p: AttractionProduct): void {
    const qty = this.qtyMap[p.productId] ?? 1;

    const checkoutPayload = {
      directBuyItems: [{
        productCode: p.productCode,
        quantity: qty,
        ticketCategoryId: p.ticketTypeCode ?? null
      }]
    };

    this.router.navigate(['/order'], { state: { data: checkoutPayload } });
  }
}
