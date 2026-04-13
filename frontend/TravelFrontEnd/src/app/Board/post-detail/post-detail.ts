import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
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
import Swal from 'sweetalert2';
import { ArticleData } from '../interface/ArticleData';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CloudinaryServe } from '../Service/cloudinary-serve';
import { reportLog } from '../Components/article-list/article-list';

registerLocaleData(localeZhTw);
interface ArticleTag {
  tagID: number;
  tagName: string;
  icon: string;
}

@Component({
  selector: 'app-post-detail',
  imports: [RouterModule, Sidebar, CommentsArea, CreateArticleButton, PopularPost, TagClouds, PostCatgories, AuthorInfoSidebar, DatePipe, ReactiveFormsModule],
  templateUrl: './post-detail.html',
  styleUrl: './post-detail.css',
})
export class PostDetail implements OnInit {
  constructor(private Serve: BoardServe, private route: ActivatedRoute, private router: Router, private _CloudinaryServe: CloudinaryServe) { };
  id = 0;
  selectedIndex = 0;
  allPhotoList: string[] = [];
  TagsList: ArticleTag[] = [];
  post?: PostDetailDto;
  product?: any;
  @ViewChild('fileInput') fileInput!: ElementRef;

  reportTarget?: PostDetailDto;
  reportForm = new FormGroup({
    violationType: new FormControl('', Validators.required),
    reportDetails: new FormControl('')
  });
  reportImageFile?: File;
  reportPhotoView?: string;

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
          if (err.status === 401) {
            Swal.fire({
              icon: "warning",
              title: "請先登入",
              timer: 1500
            });
            this.router.navigate(['/login']);
          }
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

  openReport() {
    if (this.post) {
      this.reportTarget = this.post;
    }
  }

  async ReportArticle() {
    if (this.reportForm.invalid) return;
    Swal.fire({
      title: "上傳中...",
      allowOutsideClick: false,
      didOpen: () => {
        Swal.showLoading();
      }
    });
    if (this.reportTarget) {
      let imageUrl = null;
      if (this.reportImageFile) {
        imageUrl = await this._CloudinaryServe.uploadImage(this.reportImageFile);
      }

      const log: reportLog = {
        TargetID: this.id,
        ViolationType: Number(this.reportForm.value.violationType) ?? 0,
        Reason: this.reportForm.value.reportDetails ?? undefined,
        Photo: imageUrl,
      }

      console.log(log);

      this.Serve.postReport(log).subscribe(
        {
          next: (d) => {
            Swal.fire({
              text: "檢舉成功!",
              icon: "success",
              showConfirmButton: false,
              timer: 1500
            });
            this.ReportFormReset();
          },
          error: (err) => {
            // 失敗
            console.error(err);
          }
        });
    }
  }

  onFileChange(event: any) {
    const file = event.target.files[0];
    this.reportImageFile = file;
    this.reportPhotoView = URL.createObjectURL(file);
  }
  ReportFormReset() {
    this.reportTarget = undefined;
    this.reportPhotoView = undefined;
    this.reportImageFile = undefined;
    this.reportForm.reset({ violationType: '' });
    this.fileInput.nativeElement.value = '';
    const modalEl = document.getElementById('ReportBackdrop');
    (window as any).bootstrap.Modal.getInstance(modalEl)?.hide();
  }
}
//  class="reaction-item" [class.liked]="post?.isLike"
