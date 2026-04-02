import { Component, OnInit } from '@angular/core';
import { BoardServe } from '../Service/board-serve';
import { ActivatedRoute, Router } from '@angular/router';
import { ArticleData, TagDTO } from '../interface/ArticleData';
import localeZhTw from '@angular/common/locales/zh-Hant';
import { DatePipe, registerLocaleData } from '@angular/common';
import { ArticleList } from "../Components/article-list/article-list";
import Swal from 'sweetalert2';

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
      this.Serve.getAuthorUser(this.UserId).subscribe({
        next: (d) => {
          if (d == null) {
            this.router.navigate(['Board/Main']);
          }
          this.curUser = d;
        },
        error: () => { this.router.navigate(['Board/404']); }
      });
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
    if (!this.UserId) return;
    Swal.fire({
      title: "封鎖中...",
      allowOutsideClick: false,
      didOpen: () => {
        Swal.showLoading();
      }
    });
    this.Serve.postBlock(this.UserId).subscribe({
      next: () => {
        let timerInterval: any;
        Swal.fire({
          title: `封鎖用戶 ${this.curUser.name} 成功！`,
          html: '即將於 <b></b> 秒後返回所有文章',
          icon: 'success',
          allowOutsideClick: false,
          confirmButtonText: '返回所有文章',
          timer: 3000,
          timerProgressBar: true,
          didOpen: () => {
            const b = Swal.getPopup()!.querySelector('b')!;
            timerInterval = setInterval(() => {
              b.textContent = `${Math.ceil(Swal.getTimerLeft()! / 1000)}`;
            }, 100);
          },
          willClose: () => {
            clearInterval(timerInterval);
          }
        }).then((result) => {
          if (result.isConfirmed || result.dismiss === Swal.DismissReason.timer) {
            this.router.navigate(['Board']);
          }
        });
      }
    }
    );
  }
}
