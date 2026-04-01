import { Component, EventEmitter, Input, Output } from '@angular/core';
import { ArticleData } from '../../interface/ArticleData';
import { BoardServe } from '../../Service/board-serve';
import { ActivatedRoute, Router } from '@angular/router';
import localeZhTw from '@angular/common/locales/zh-Hant';
import { DatePipe, registerLocaleData } from '@angular/common';

registerLocaleData(localeZhTw);
@Component({
  selector: 'app-article-list',
  imports: [DatePipe,],
  templateUrl: './article-list.html',
  styleUrl: './article-list.css',
})
export class ArticleList {
  constructor(private Serve: BoardServe, private route: ActivatedRoute, private router: Router) {
  }
  @Input() articleList: ArticleData[] = [];
  @Output() tagSelected = new EventEmitter<number>();

  goToDetail(id: number): void {
    this.router.navigate(['Board', 'detail', id]);
  }

  goToMemderPage(memderID: string): void {
    this.router.navigate(['Board', 'user', memderID]);
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

  seachTag(tag: number) {
    console.log('seachTag emit', tag);
    this.tagSelected.emit(tag);
  }
}
