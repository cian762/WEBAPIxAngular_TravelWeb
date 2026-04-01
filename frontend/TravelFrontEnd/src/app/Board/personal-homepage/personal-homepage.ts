import { Subject } from 'rxjs';
import { Component, OnInit } from '@angular/core';
import { BoardServe } from '../Service/board-serve';
import { ActivatedRoute, Router } from '@angular/router';
import { ArticleData, TagDTO } from '../interface/ArticleData';
import localeZhTw from '@angular/common/locales/zh-Hant';
import { DatePipe, registerLocaleData } from '@angular/common';
import { CreateArticleButton } from "../Components/create-article-button/create-article-button";
import { ArticleList } from "../Components/article-list/article-list";

registerLocaleData(localeZhTw);

@Component({
  selector: 'app-personal-homepage',
  imports: [DatePipe, CreateArticleButton, ArticleList],
  templateUrl: './personal-homepage.html',
  styleUrl: './personal-homepage.css',
})
export class PersonalHomepage implements OnInit {
  constructor(private Serve: BoardServe, private route: ActivatedRoute, private router: Router) {
  }
  articleList: ArticleData[] = [];
  collectList: ArticleData[] = [];
  AllTags: TagDTO[] = [];
  curUser: any;
  totalCount = 0;

  ngOnInit(): void {
    this.Serve.getCurUser().subscribe(d => this.curUser = d);
    this.Serve.getArticleByUserAPI(1).subscribe(p => this.articleList = p.articleList);
    this.Serve.getAllTags().subscribe(p => this.AllTags = p);
    this.Serve.getArticleByCollect(1).subscribe(p => {
      this.collectList = p.articleList;
      this.totalCount = p.totalCount
    })
  }


  goToDetail(id: number): void {
    this.router.navigate(['Board', 'detail', id]);
  }

  goToMemderPage(memderID: string): void {
    this.router.navigate(['Board', 'user', memderID]);
  }

  goToUpdate(id: number): void {
    this.router.navigate(['Board', 'creat', id]);
  }

  ToLike(id: number) {
    var article = this.articleList.find(a => a.articleId === id);
    if (article) {
      if (!article.isLike) {
        article.likeCount++;
        article.isLike = true;
      }
      else {
        article.likeCount--;
        article.isLike = false;
      }
      this.Serve.postArticleLike(id).subscribe();
    }

  }


  onTagSelected(tag: any) {

  }

}

