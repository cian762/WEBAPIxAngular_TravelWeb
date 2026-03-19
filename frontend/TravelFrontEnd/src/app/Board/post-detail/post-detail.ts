import { Component, OnInit } from '@angular/core';
import { BoardServe } from '../board-serve';
import { PostDetailDto } from '../interface/PostDetailDto';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { Sidebar } from "../Components/sidebar/sidebar";

@Component({
  selector: 'app-post-detail',
  imports: [RouterModule, Sidebar],
  templateUrl: './post-detail.html',
  styleUrl: './post-detail.css',
})
export class PostDetail implements OnInit {
  constructor(private Serve: BoardServe, private route: ActivatedRoute) {

  }
  id = 0;
  ngOnInit(): void {
    this.route.paramMap.subscribe(p => {
      this.id = Number(p.get('id'));
      console.log(this.id);
      this.Serve.getArticleDetailAPI(this.id).subscribe((d) => {
        this.post = d;
      });
    });
  }

  post?: PostDetailDto;
}
