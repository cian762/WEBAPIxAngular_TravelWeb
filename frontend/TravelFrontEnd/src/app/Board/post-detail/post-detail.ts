import { Component, OnInit } from '@angular/core';
import { BoardServe } from '../Service/board-serve';
import { PostDetailDto } from '../interface/PostDetailDto';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { Sidebar } from "../Components/sidebar/sidebar";
import { CommentsArea } from "../Components/comments-area/comments-area";
import { CreateArticleButton } from "../Components/create-article-button/create-article-button";
import { PopularPost } from "../Components/popular-post/popular-post";
import { TagClouds } from "../Components/tag-clouds/tag-clouds";
import { PostCatgories } from "../Components/post-catgories/post-catgories";
import { AuthorInfoSidebar } from "../Components/author-info-sidebar/author-info-sidebar";
import localeZhTw from '@angular/common/locales/zh-Hant';
import { DatePipe, registerLocaleData } from '@angular/common';

registerLocaleData(localeZhTw);
interface ArticleTag {
  tagID: number;
  tagName: string;
  icon: string;
}

@Component({
  selector: 'app-post-detail',
  imports: [RouterModule, Sidebar, CommentsArea, CreateArticleButton, PopularPost, TagClouds, PostCatgories, AuthorInfoSidebar, DatePipe,],
  templateUrl: './post-detail.html',
  styleUrl: './post-detail.css',
})
export class PostDetail implements OnInit {
  constructor(private Serve: BoardServe, private route: ActivatedRoute, private router: Router) { };
  id = 0;
  selectedIndex = 0;
  allPhotoList: string[] = [];
  TagsList: ArticleTag[] = [];
  post?: PostDetailDto;
  product?: any;

  ngOnInit(): void {
    this.route.paramMap.subscribe(p => {
      this.id = Number(p.get('id'));
      this.Serve.getArticleDetailAPI(this.id).subscribe({
        next: (d) => {
          {
            this.post = d;
            this.Serve.postLogView(this.id).subscribe();
            if (d.cover) {
              this.allPhotoList?.push(d.cover);
            }
            if (d.postPhoto) {
              this.allPhotoList?.push(...d.postPhoto);
              console.log(this.id);
            }

          }
        },
        error: (err: any) => {
          if (err.status === 404) {
            this.router.navigate(['Board/404']);
          }
        }
      });
      this.Serve.getTagsByArticleAPI(this.id).subscribe((d: any) => {
        this.TagsList = d;
      });
      this.Serve.getProduct(this.id).subscribe((d: any) => {
        this.product = d;
      });
    });
  }



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

  ToLike(id: number) {
    if (this.post) {
      if (!this.post.isLike) {
        this.post.likeCount++;
        this.post.isLike = true;
      }
      else {
        this.post.likeCount--;
        this.post.isLike = false;
      }
      this.Serve.postArticleLike(id).subscribe();
    }
  }

  ToCollect(id: number) {
    if (this.post) {
      if (!this.post.isCollect) {
        this.post.isCollect = true;
      }
      else {
        this.post.isCollect = false;
      }
      this.Serve.postArticleCollect(id).subscribe();
    }
  }


  // 跳轉邏輯也搬到這裡，讓原件自己處理點擊
  goToDetail(item: any) {
    const routeMap: any = {
      'Article': '/Board/detail',        // 對應 Board -> detail/:id
      'Activity': '/ActivityInfo',       // 對應 ActivityInfo -> :id
      'Attraction': '/attractions/detail', // 對應 attractions -> detail/:id
      'Product': '/trip-detail'          // 對應 trip-detail/:id
    };
    const basePath = routeMap[item.category]; // 如果你之前改成了 type，這裡記得用 type

    if (basePath && item.id) {
      // Angular navigate 會自動幫你加上斜線：/Board/detail/123
      this.router.navigate([basePath, item.id]);
    } else {
      console.error('找不到對應路由或 ID', item);
    }
  }
}
//  class="reaction-item" [class.liked]="post?.isLike"
