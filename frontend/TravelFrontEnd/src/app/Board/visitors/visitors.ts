import { Component, OnInit } from '@angular/core';
import { ArticleList } from "../Components/article-list/article-list";
import { ArticleData } from '../interface/ArticleData';
import { BoardServe } from '../Service/board-serve';

@Component({
  selector: 'app-visitors',
  imports: [ArticleList],
  templateUrl: './visitors.html',
  styleUrl: './visitors.css',
})
export class Visitors implements OnInit {
  constructor(private Serve: BoardServe) { }
  ngOnInit(): void {
    this.Serve.getArtcleForVister().subscribe(d =>
      this.articleList = d
    );
  }
  articleList: ArticleData[] = []
}
