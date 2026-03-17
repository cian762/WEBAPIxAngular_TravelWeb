import { Component, OnInit } from '@angular/core';
import { BoardServe } from '../board-serve';
import { PostDetailDto } from '../interface/PostDetailDto';

@Component({
  selector: 'app-post-detail',
  imports: [],
  templateUrl: './post-detail.html',
  styleUrl: './post-detail.css',
})
export class PostDetail implements OnInit {
  constructor(private Serve: BoardServe) {

  }
  ngOnInit(): void {
    this.Serve.getArticleDetailAPI(2).subscribe((d) => {
      this.post = d;
    });
  }

  post?: PostDetailDto;
}
