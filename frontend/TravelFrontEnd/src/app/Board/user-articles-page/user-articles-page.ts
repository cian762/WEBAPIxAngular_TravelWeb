import { Component, OnInit } from '@angular/core';
import { BoardServe } from '../Service/board-serve';
import { ActivatedRoute, Router } from '@angular/router';
import { ArticleData, TagDTO } from '../interface/ArticleData';
import localeZhTw from '@angular/common/locales/zh-Hant';
import { DatePipe, registerLocaleData } from '@angular/common';

registerLocaleData(localeZhTw);

@Component({
  selector: 'app-user-articles-page',
  imports: [DatePipe],
  templateUrl: './user-articles-page.html',
  styleUrl: './user-articles-page.css',
})
export class UserArticlesPage implements OnInit {
  constructor(private Serve: BoardServe, private route: ActivatedRoute, private router: Router) {
  }
  articleList: ArticleData[] = [];
  AllTags: TagDTO[] = [];
  UserId: string = "";
  curUser: any;

  ngOnInit(): void {
    this.route.paramMap.subscribe(p => {
      this.UserId = p.get('id')!;
      this.Serve.getAuthorUser(this.UserId).subscribe((d) => { this.curUser = d; });
    });
    this.Serve.getArticleByAuthor(1, this.UserId).subscribe(p => this.articleList = p.articleList);
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
