import { Component, OnInit } from '@angular/core';
import { BoardServe } from '../Service/board-serve';
import { ActivatedRoute, Router } from '@angular/router';
import { JournalDetailDTO } from '../interface/JournalDetailDTO';
import { CreateArticleButton } from "../Components/create-article-button/create-article-button";
import { CommentsArea } from "../Components/comments-area/comments-area";
import { PopularPost } from "../Components/popular-post/popular-post";
import { TagClouds } from "../Components/tag-clouds/tag-clouds";
import { PostCatgories } from "../Components/post-catgories/post-catgories";

import { DatePipe, registerLocaleData } from '@angular/common';
import localeZhTw from '@angular/common/locales/zh-Hant';

@Component({
  selector: 'app-journal-detail',
  imports: [CreateArticleButton, CommentsArea, PopularPost, TagClouds, PostCatgories, DatePipe,],
  templateUrl: './journal-detail.html',
  styleUrl: './journal-detail.css',
})
export class JournalDetail implements OnInit {
  constructor(private Serve: BoardServe, private route: ActivatedRoute, private router: Router) { };
  id = 0;
  journal?: JournalDetailDTO;


  ngOnInit(): void {
    this.route.paramMap.subscribe(p => {
      this.id = Number(p.get('id'));
      this.Serve.getJournalDetailAPI(this.id).subscribe({
        next: (d) => {
          {
            this.journal = d;
          }
        },
        error: (err: any) => {
          if (err.status === 404) {
            this.router.navigate(['Board/404']);
          }
        }
      });
    });
  }



  ToLike(id: number) {
    if (this.journal) {
      if (!this.journal.isLike) {
        this.journal.likeCount++;
        this.journal.isLike = true;
      }
      else {
        this.journal.likeCount--;
        this.journal.isLike = false;
      }
      this.Serve.postArticleLike(id).subscribe();
    }
  }

  ToCollect(id: number) {
    if (this.journal) {
      if (!this.journal.isCollect) {
        this.journal.isCollect = true;
      }
      else {
        this.journal.isCollect = false;
      }
      this.Serve.postArticleCollect(id).subscribe();
    }
  }
}
