import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { Tripproduct } from '../../services/tripproduct';
import { ProductQueryDTO, RegionListDTO, TagListDTO, TripProductDTO } from '../../models/tripproduct.model';
import { FormsModule } from '@angular/forms';



@Component({
  selector: 'app-product',
  imports: [CommonModule, FormsModule, HttpClientModule],
  templateUrl: './product.html',
  styleUrl: './product.css',
})
export class Product implements OnInit {
  constructor(private tripService: Tripproduct) { }
  isExpanded = false; // 控制標籤是否展開
  displayLimit = 10;
  // 存放下拉選單與按鈕資料
  regions: RegionListDTO[] = [];
  tags: TagListDTO[] = [];

  // 存放搜尋結果
  products: TripProductDTO[] = [];
  totalCount: number = 0;

  // 搜尋條件 (繫結到 HTML 表單)
  query: ProductQueryDTO = {
    page: 1,
    pageSize: 16,
    keyword: '',
    tagIds: []
  };
  toggleExpand() {
    this.isExpanded = !this.isExpanded;
  }
  ngOnInit(): void {
    // 1. 頁面載入時先拿選單資料
    this.tripService.getMetaData().subscribe(res => {
      this.regions = res.regions;
      this.tags = res.tags;
    });
    // 2. 預設執行一次搜尋 (顯示第一頁)
    this.onSearch();
  };
  // 觸發搜尋
  onSearch(): void {
    this.tripService.searchProducts(this.query).subscribe(res => {
      this.products = res.data || [];
      this.totalCount = res.totalCount;
    });
  }

  // 分頁切換時觸發
  onPageChange(newPage: number): void {
    this.query.page = newPage;
    this.onSearch();
  }
  toggleTag(tagId: number) {
    if (!this.query.tagIds) this.query.tagIds = [];

    const index = this.query.tagIds.indexOf(tagId);
    if (index > -1) {
      this.query.tagIds.splice(index, 1);
    } else {
      this.query.tagIds.push(tagId);
    }
    this.query.page = 1; // 篩選變動時回到第一頁
    this.onSearch();
  }


}
