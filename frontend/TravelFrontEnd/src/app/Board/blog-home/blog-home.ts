import { ChangeDetectorRef, Component, Input, input, OnInit } from '@angular/core';
import { ArticleData, ArticleResponse } from '../interface/ArticleData';
import { BoardServe } from '../Service/board-serve';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PopularPost } from "../Components/popular-post/popular-post";
import { TagClouds } from "../Components/tag-clouds/tag-clouds";

import { ArticleList } from "../Components/article-list/article-list";
import { CreateArticleButton } from "../Components/create-article-button/create-article-button";
import { PostCatgories } from "../Components/post-catgories/post-catgories";
import { PageNumberList } from "../Components/page-number-list/page-number-list";



@Component({
  selector: 'app-blog-home',
  imports: [RouterModule, FormsModule, PopularPost, TagClouds, ArticleList, CreateArticleButton, PostCatgories, PageNumberList],
  templateUrl: './blog-home.html',
  styleUrl: './blog-home.css',
})
export class BlogHome implements OnInit {
  constructor(private Serve: BoardServe, private route: ActivatedRoute, private router: Router,
    private cdr: ChangeDetectorRef
  ) {

  }
  articleList: ArticleData[] = history.state.articleList || [];
  totalCount = 0;
  currentPage: number = 1;
  Keyword = "";
  isSearch = false;

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      console.log('queryParams triggered', params);
      const tagId = params['TagsId'];
      if (tagId) {
        this.isSearch = true;
        const para: string[] = [];
        para.push(`TagsId=${tagId}`);
        this.Serve.getArticleByTags(1, false, para).subscribe((d: any) => {
          this.articleList = d.articleList;
          this.totalCount = d.totalCount;
          this.cdr.markForCheck();
        });
      } else {
        this.ReflashArticles(this.currentPage);
      }
    });
  }

  ReflashArticles(currentPage: number) {
    this.Serve.getArticleAPI(currentPage).subscribe((d: ArticleResponse) => {
      this.articleList = d.articleList;
      this.totalCount = d.totalCount;
      this.isSearch = false;
      console.log("totalCount", this.totalCount);
    });
  }



  searchByKeyword() {
    if (this.Keyword == "") {
      this.ReflashArticles(1);
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

  onTagSelected(tag: any) {
    const para = [`TagsId=${tag}`];
    this.Serve.getArticleByTags(1, false, para).subscribe((d: any) => {
      this.articleList = [...d.articleList];
      this.totalCount = d.totalCount;
      this.isSearch = true;
    });
  }

}
