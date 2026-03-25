import { Component, Input, input, OnInit } from '@angular/core';
import { ArticleData, ArticleResponse } from '../interface/ArticleData';
import { BoardServe } from '../Service/board-serve';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PopularPost } from "../Components/popular-post/popular-post";
import { TagClouds } from "../Components/tag-clouds/tag-clouds";


@Component({
  selector: 'app-blog-home',
  imports: [RouterModule, FormsModule, PopularPost, TagClouds],
  templateUrl: './blog-home.html',
  styleUrl: './blog-home.css',
})
export class BlogHome implements OnInit {
  constructor(private Serve: BoardServe, private router: Router
  ) {

  }
  articleList: ArticleData[] = history.state.articleList || [];
  totalCount = 0;
  currentPage: number = 1;
  Keyword = "";

  ngOnInit(): void {
    if (this.articleList.length === 0) {
      this.ReflashArticles();
    }

  }


  // 計算總頁數並轉成陣列 [1, 2, 3...]
  get pageNumbers(): number[] {
    const totalPages = Math.ceil(this.totalCount / 10);
    // 產生一個長度為 totalPages 的陣列
    return Array.from({ length: totalPages }, (_, i) => i + 1);
  }

  ReflashArticles() {
    this.Serve.getArticleAPI(this.currentPage).subscribe((d: ArticleResponse) => {
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
    this.Serve.postPostAPI().subscribe(p => {
      this.router.navigate(['Board', 'creat', p]);
    });
  }

  goToUpdate(id: number): void {
    this.router.navigate(['Board', 'creat', id]);
  }

  onFocus(event: any) {
    event.target.closest('.search-box').classList.add('focused');
    if (event.target.value) {
      this.onSeach(event);
    }
  }

  onSeach(event: any) {
    const keyword = event.target.value;

  }



  onBlur(event: any) {
    setTimeout(() => {
      event.target.closest('.search-box').classList.remove('focused');
    }, 200);
  }

}
