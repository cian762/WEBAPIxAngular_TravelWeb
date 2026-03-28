import { Component, OnInit } from '@angular/core';
import { BoardServe } from '../Service/board-serve';
import { ActivatedRoute, Router } from '@angular/router';
import { ArticleData } from '../interface/ArticleData';

@Component({
  selector: 'app-user-articles-page',
  imports: [],
  templateUrl: './user-articles-page.html',
  styleUrl: './user-articles-page.css',
})
export class UserArticlesPage implements OnInit {
  constructor(private Serve: BoardServe, private route: ActivatedRoute, private router: Router) {
  }

  MemderID = "";
  articleList: ArticleData[] = history.state.articleList || [];


  ngOnInit(): void {
    this.route.paramMap.subscribe(p => {
      this.MemderID = String(p.get('id'));
      this.Serve.getArticleByAuthor(1, this.MemderID).subscribe((d) => {
        this.articleList = d.articleList;
      });
    });
  }
}
