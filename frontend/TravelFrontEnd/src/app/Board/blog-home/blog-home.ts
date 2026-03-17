import { Component, OnInit } from '@angular/core';
import { ArticleData } from '../interface/ArticleData';
import { BoardServe } from '../board-serve';

@Component({
  selector: 'app-blog-home',
  imports: [],
  templateUrl: './blog-home.html',
  styleUrl: './blog-home.css',
})
export class BlogHome implements OnInit {

  constructor(private Serve: BoardServe) {

  }

  ngOnInit(): void {
    this.Serve.getArticleAPI().subscribe((d) => {
      this.articleList = d;
      console.log("d", d);
      console.log("articleList", this.articleList);
    });

    console.log("122222222");
  }
  articleList: ArticleData[] = [
  ]


}
