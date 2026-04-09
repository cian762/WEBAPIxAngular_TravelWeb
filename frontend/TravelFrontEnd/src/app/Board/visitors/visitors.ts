import { Component, OnInit } from '@angular/core';
import { ArticleList } from "../Components/article-list/article-list";
import { ArticleData } from '../interface/ArticleData';
import { BoardServe } from '../Service/board-serve';
import { Router } from '@angular/router';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-visitors',
  imports: [ArticleList],
  templateUrl: './visitors.html',
  styleUrl: './visitors.css',
})
export class Visitors implements OnInit {
  constructor(private Serve: BoardServe, private router: Router) { }
  ngOnInit(): void {
    this.Serve.getArtcleForVister().subscribe(d =>
      this.articleList = d
    );
  }
  articleList: ArticleData[] = []

  gotoLogin() {
    Swal.fire({
      icon: "warning",
      title: "請先登入",
      timer: 1500
    });
    this.router.navigate(['login']);
  }
}
