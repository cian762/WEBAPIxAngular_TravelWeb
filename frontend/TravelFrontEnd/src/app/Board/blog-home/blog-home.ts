import { Component, OnInit } from '@angular/core';
import { ArticleData } from '../interface/ArticleData';
import { BoardServe } from '../board-serve';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-blog-home',
  imports: [RouterModule, FormsModule],
  templateUrl: './blog-home.html',
  styleUrl: './blog-home.css',
})
export class BlogHome implements OnInit {
  constructor(private Serve: BoardServe,
    private router: Router
  ) {

  }
  articleList: ArticleData[] = [
  ]
  totalCount = 0;
  currentPage: number = 1;
  Keyword = "";

  ngOnInit(): void {
    this.ReflashArticles();
  }


  // 計算總頁數並轉成陣列 [1, 2, 3...]
  get pageNumbers(): number[] {
    const totalPages = Math.ceil(this.totalCount / 10);
    // 產生一個長度為 totalPages 的陣列
    return Array.from({ length: totalPages }, (_, i) => i + 1);
  }

  ReflashArticles() {
    this.Serve.getArticleAPI(this.currentPage).subscribe((d: any) => {
      this.articleList = d.articleList;
      this.totalCount = d.totalCount;
    });
  }

  // 點擊換頁的方法
  changePage(p: number, event: Event) {
    event.preventDefault(); // 防止 <a> 標籤跳轉
    this.currentPage = p;
    this.ReflashArticles(); // 重新去後端抓那一頁的資料
  }

  searchByKeyword() {
    if (this.Keyword == "") {
      this.ReflashArticles();
    }
    else {
      this.Serve.getArticleByKeyword(1, this.Keyword).subscribe((d: any) => {
        this.articleList = d.articleList;
        this.totalCount = d.totalCount;
      });
    }

  }

  goToDetail(id: number): void {
    console.log(id);
    this.router.navigate(['Board', 'detail', id]);
  }

  goToCreate(): void {
    this.Serve.postPostAPI("Turtle_05").subscribe(p => {
      this.router.navigate(['Board', 'creat', p]);
    });
  }

  goToUpdate(id: number): void {
    this.router.navigate(['Board', 'creat', id]);
  }



}
