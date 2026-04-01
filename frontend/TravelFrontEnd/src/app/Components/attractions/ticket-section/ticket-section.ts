import { Component, Input, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AttractionService, AttractionProduct, ProductDetailInfo } from '../attraction.service';
import { CreateShoppingCart } from '../../../trip/services/create-shopping-cart';
import { AuthService } from '../../../Member/services/auth.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-ticket-section',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './ticket-section.html',
  styleUrls: ['./ticket-section.css']
})
export class TicketSectionComponent implements OnInit, OnChanges {
  @Input() attractionId!: number;
  @Input() coverImage: string = '';
  @Input() attractionName: string = '';

  loading = true;
  products: AttractionProduct[] = [];

  stockMap: Record<string, number> = {};
  stockLoaded: Record<string, boolean> = {};
  qtyMap: Record<number, number> = {};
  expandedId: number | null = null;

  drawerOpen = false;
  drawerLoading = false;
  drawerDetail: ProductDetailInfo | null = null;

  // productId → 是否已收藏
  favoriteMap: Record<number, boolean> = {};
  // productId → 動畫狀態：'add' | 'remove' | null
  favAnimMap: Record<number, string | null> = {};

  constructor(
    private svc: AttractionService,
    private cartService: CreateShoppingCart,
    private router: Router,
    private authService: AuthService
  ) { }

  ngOnInit(): void {
    this.loadProducts();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['attractionId'] && !changes['attractionId'].firstChange) {
      this.loadProducts();
    }
  }

  private loadProducts(): void {
    this.loading = true;
    this.products = [];
    this.stockMap = {};
    this.stockLoaded = {};
    this.qtyMap = {};
    this.expandedId = null;
    this.drawerOpen = false;
    this.drawerDetail = null;
    this.favoriteMap = {};
    this.favAnimMap = {};

    this.svc.getProductsByAttraction(this.attractionId).subscribe(list => {
      this.products = list;
      this.loading = false;

      list.forEach(p => {
        this.qtyMap[p.productId] = 1;
        this.favAnimMap[p.productId] = null;
        this.svc.getStock(p.productCode).subscribe(s => {
          this.stockMap[p.productCode] = s.remainingStock;
          this.stockLoaded[p.productCode] = true;
        });
      });

      // 登入狀態才載入收藏清單
      if (this.authService.isLoggedIn()) {
        this.svc.getMyFavorites().subscribe(ids => {
          ids.forEach(id => this.favoriteMap[id] = true);
        });
      }
    });
  }

  // ── 收藏 toggle ───────────────────────────────────────

  toggleFavorite(p: AttractionProduct, event: Event): void {
    event.stopPropagation(); // 不觸發展開

    if (!this.authService.isLoggedIn()) {
      Swal.fire({
        toast: true,
        position: 'top',
        icon: 'warning',
        title: '請先登入才能收藏票券',
        showConfirmButton: false,
        timer: 2500
      });
      return;
    }

    const wasFavorited = !!this.favoriteMap[p.productId];

    // 先播動畫
    this.favAnimMap[p.productId] = wasFavorited ? 'remove' : 'add';
    setTimeout(() => this.favAnimMap[p.productId] = null, 600);

    // 樂觀更新 UI
    this.favoriteMap[p.productId] = !wasFavorited;

    this.svc.toggleFavorite(p.productId).subscribe({
      next: (res) => {
        this.favoriteMap[p.productId] = res.isFavorited;
      },
      error: () => {
        // 失敗時還原
        this.favoriteMap[p.productId] = wasFavorited;
        Swal.fire({
          toast: true, position: 'top', icon: 'error',
          title: '操作失敗，請稍後再試', showConfirmButton: false, timer: 2000
        });
      }
    });
  }

  isFavorited(productId: number): boolean {
    return !!this.favoriteMap[productId];
  }

  getFavAnim(productId: number): string | null {
    return this.favAnimMap[productId] ?? null;
  }

  // ── Accordion ─────────────────────────────────────────

  toggleExpand(productId: number): void {
    this.expandedId = this.expandedId === productId ? null : productId;
  }

  isExpanded(productId: number): boolean {
    return this.expandedId === productId;
  }

  // ── Drawer ────────────────────────────────────────────

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
    return this.stockLoaded[productCode] &&
      (this.stockMap[productCode] ?? 0) < 10 &&
      (this.stockMap[productCode] ?? 0) > 0;
  }

  isSoldOut(productCode: string): boolean {
    return this.stockLoaded[productCode] && (this.stockMap[productCode] ?? 0) <= 0;
  }

  // ── 工具 ──────────────────────────────────────────────

  toLines(text: string | null | undefined): string[] {
    if (!text) return [];
    return text.replace(/\\n/g, '\n').split('\n')
      .map(s => s.trim()).filter(s => s.length > 0);
  }

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
      attractionId: this.attractionId,
      attractionName: this.attractionName,
      tags: p.tags?.map(t => t.tagName) ?? [],
    };
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
