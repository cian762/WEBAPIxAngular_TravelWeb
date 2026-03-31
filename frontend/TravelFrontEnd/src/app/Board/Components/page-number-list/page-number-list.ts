import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-page-number-list',
  imports: [],
  templateUrl: './page-number-list.html',
  styleUrl: './page-number-list.css',
})
export class PageNumberList {
  @Input() totalCount: number = 1;
  @Input() currentPage: number = 1;
  @Output() pageChanged = new EventEmitter<number>();

  // 計算總頁數並轉成陣列 [1, 2, 3...]
  get pageNumbers(): number[] {
    const totalPages = Math.ceil(this.totalCount / 10);
    // 產生一個長度為 totalPages 的陣列
    return Array.from({ length: totalPages }, (_, i) => i + 1);
  }
  // 點擊換頁的方法
  changePage(p: number, event: Event) {
    event.preventDefault(); // 防止 <a> 標籤跳轉
    this.currentPage = p;
    this.pageChanged.emit(p); // 重新去後端抓那一頁的資料
  }

}
