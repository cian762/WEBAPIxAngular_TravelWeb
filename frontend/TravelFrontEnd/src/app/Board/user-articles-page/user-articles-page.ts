import { Component, OnInit } from '@angular/core';
import { BoardServe } from '../Service/board-serve';
import { ActivatedRoute, Router } from '@angular/router';
import { ArticleData, TagDTO } from '../interface/ArticleData';
import localeZhTw from '@angular/common/locales/zh-Hant';
import { DatePipe, registerLocaleData } from '@angular/common';
import { ArticleList } from "../Components/article-list/article-list";

registerLocaleData(localeZhTw);

@Component({
  selector: 'app-user-articles-page',
  imports: [DatePipe, ArticleList],
  templateUrl: './user-articles-page.html',
  styleUrl: './user-articles-page.css',
})
export class UserArticlesPage implements OnInit {
  constructor(private Serve: BoardServe, private route: ActivatedRoute, private router: Router) {
  }
  articleList: ArticleData[] = [];
  totalCount: number = 0;
  UserId: string = "";
  curUser: any;
  isFollowing = false;

  ngOnInit(): void {
    this.route.paramMap.subscribe(p => {
      this.UserId = p.get('id')!;
      this.Serve.getAuthorUser(this.UserId).subscribe((d) => { this.curUser = d; });
    });
    this.Serve.getArticleByAuthor(1, this.UserId).subscribe((p) => {
      this.articleList = p.articleList;
      this.totalCount = p.totalCount;
    });
    this.Serve.getIsFollowing(this.UserId).subscribe(d => this.isFollowing = d);

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

  ToFollow() {
    this.Serve.postFollow(this.UserId).subscribe({
      next: (res: any) => {
        this.isFollowing = !this.isFollowing;
      },
      error: (err) => {
        console.error(err);
      }
    });
  }



  onTagSelected(tag: any) {

  }

  toBlock() {
    if (this.UserId)
      this.Serve.postBlock(this.UserId).subscribe();
  }
}
