import { Component, Input, input, OnInit } from '@angular/core';
import { ArticleData, ArticleResponse } from '../interface/ArticleData';
import { BoardServe } from '../Service/board-serve';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PopularPost } from "../Components/popular-post/popular-post";
import { TagClouds } from "../Components/tag-clouds/tag-clouds";

import { ArticleList } from "../Components/article-list/article-list";
import { CreateArticleButton } from "../Components/create-article-button/create-article-button";
import { PostCatgories } from "../Components/post-catgories/post-catgories";



@Component({
  selector: 'app-blog-home',
  imports: [RouterModule, FormsModule, PopularPost, TagClouds, ArticleList, CreateArticleButton, PostCatgories],
  templateUrl: './blog-home.html',
  styleUrl: './blog-home.css',
})
export class BlogHome implements OnInit {
  constructor(private Serve: BoardServe, private route: ActivatedRoute, private router: Router
  ) {

  }
  articleList: ArticleData[] = history.state.articleList || [];
  totalCount = 0;
  currentPage: number = 1;
  Keyword = "";

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      if (params['TagsId']) {
        const para: string[] = [];
        para.push(`TagsId=${params['TagsId']}`);
        this.Serve.getArticleByTags(1, false, para).subscribe((d: any) => {
          this.articleList = d.articleList;
          this.totalCount = d.totalCount;
        });
      } else {
        this.ReflashArticles();
      }
    });
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
