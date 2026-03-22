import { Component, OnInit } from '@angular/core';
import { BoardServe } from '../board-serve';
import { PostDetailDto } from '../interface/PostDetailDto';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { Sidebar } from "../Components/sidebar/sidebar";
import { CommentsArea } from "../Components/comments-area/comments-area";

@Component({
  selector: 'app-post-detail',
  imports: [RouterModule, Sidebar, CommentsArea],
  templateUrl: './post-detail.html',
  styleUrl: './post-detail.css',
})
export class PostDetail implements OnInit {
  constructor(private Serve: BoardServe, private route: ActivatedRoute) {

  }
  id = 0;
  selectedIndex = 0;
  allPhotoList: string[] = [];

  ngOnInit(): void {
    this.route.paramMap.subscribe(p => {
      this.id = Number(p.get('id'));
      this.Serve.getArticleDetailAPI(this.id).subscribe((d) => {
        this.post = d;
        if (d.cover) {
          this.allPhotoList?.push(d.cover);
        }
        if (d.postPhoto) {
          this.allPhotoList?.push(...d.postPhoto);
          console.log(this.id);
        }

      });
    });
  }

  post?: PostDetailDto;

  setIndex(index: number) {
    this.selectedIndex = index;
  }
}
