import { Component, OnInit } from '@angular/core';
import { ArticleData } from '../interface/ArticleData';
import { BoardServe } from '../board-serve';
import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-blog-home',
  imports: [RouterModule],
  templateUrl: './blog-home.html',
  styleUrl: './blog-home.css',
})
export class BlogHome implements OnInit {


  constructor(private Serve: BoardServe,
    private router: Router
  ) {

  }

  ngOnInit(): void {
    this.Serve.getArticleAPI().subscribe((d) => {
      this.articleList = d;
      console.log("d", d);
      console.log("articleList", this.articleList);
    });

  }
  articleList: ArticleData[] = [
  ]

  goToDetail(id: number): void {
    console.log(id);
    this.router.navigate(['Board', 'detail', id]);
  }

  goToCreate(): void {
    this.Serve.postPostAPI("Turtle_05").subscribe(p => {
      this.router.navigate(['Board', 'creat', p]);
    });
  }

  goToUpdate(id: number): void {
    this.router.navigate(['Board', 'creat', id]);
  }


}
