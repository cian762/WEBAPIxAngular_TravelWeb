import { Component, OnInit } from '@angular/core';
import { BoardServe } from '../Service/board-serve';
import { ActivatedRoute, Router } from '@angular/router';
import { ArticleData, TagDTO } from '../interface/ArticleData';
import localeZhTw from '@angular/common/locales/zh-Hant';
import { DatePipe, registerLocaleData } from '@angular/common';

registerLocaleData(localeZhTw);

@Component({
  selector: 'app-personal-homepage',
  imports: [DatePipe],
  templateUrl: './personal-homepage.html',
  styleUrl: './personal-homepage.css',
})
export class PersonalHomepage implements OnInit {
  constructor(private Serve: BoardServe, private route: ActivatedRoute, private router: Router) {
  }
  articleList: ArticleData[] = [];
  AllTags: TagDTO[] = [];
  ngOnInit(): void {
    this.Serve.getArticleByUserAPI(1).subscribe(p => this.articleList = p.articleList);
    this.Serve.getAllTags().subscribe(p => this.AllTags = p);
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

}

