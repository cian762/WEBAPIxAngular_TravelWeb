import { Component, OnInit } from '@angular/core';
import { BoardServe } from '../Service/board-serve';
import { PostDetailDto } from '../interface/PostDetailDto';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { Sidebar } from "../Components/sidebar/sidebar";
import { CommentsArea } from "../Components/comments-area/comments-area";
import { CreateArticleButton } from "../Components/create-article-button/create-article-button";


interface ArticleTag {
  tagID: number;
  tagName: string;
  icon: string;
}

@Component({
  selector: 'app-post-detail',
  imports: [RouterModule, Sidebar, CommentsArea, CreateArticleButton],
  templateUrl: './post-detail.html',
  styleUrl: './post-detail.css',
})
export class PostDetail implements OnInit {
  constructor(private Serve: BoardServe, private route: ActivatedRoute, private router: Router) {

  }
  id = 0;
  selectedIndex = 0;
  allPhotoList: string[] = [];
  TagsList: ArticleTag[] = [];

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
      this.Serve.getTagsByArticleAPI(this.id).subscribe((d: any) => {
        this.TagsList = d;
      })
    });
  }

  post?: PostDetailDto;

  setIndex(index: number) {
    this.selectedIndex = index;
  }

  seachTag(tag: any) {
    console.log(tag)
    const para: string[] = [];
    para.push(`TagsId=${tag.tagId}&`);
    this.Serve.getArticleByTags(1, false, para).subscribe((d: any) => {
      this.router.navigate(['Board'], {
        state: { articleList: d.articleList }
      });
    }
    )
  }
}
