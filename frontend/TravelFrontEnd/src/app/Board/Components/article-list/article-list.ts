import { Component, ElementRef, EventEmitter, Input, Output, ViewChild } from '@angular/core';
import { ArticleData } from '../../interface/ArticleData';
import { BoardServe } from '../../Service/board-serve';
import { ActivatedRoute, Router } from '@angular/router';
import localeZhTw from '@angular/common/locales/zh-Hant';
import { DatePipe, registerLocaleData } from '@angular/common';
import Swal from 'sweetalert2';
import { CloudinaryServe } from '../../Service/cloudinary-serve';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { ReactiveFormsModule } from '@angular/forms';
registerLocaleData(localeZhTw);

export interface reportLog {
  TargetID: number;
  ViolationType: number;
  Reason?: string
  Photo?: string
}


@Component({
  selector: 'app-article-list',
  imports: [DatePipe, ReactiveFormsModule],
  templateUrl: './article-list.html',
  styleUrl: './article-list.css',
})
export class ArticleList {
  constructor(private Serve: BoardServe, private route: ActivatedRoute, private router: Router, private _CloudinaryServe: CloudinaryServe) {
  }
  @Input() articleList: ArticleData[] = [];
  @Output() tagSelected = new EventEmitter<number>();
  @ViewChild('fileInput') fileInput!: ElementRef;
  reportTarget?: ArticleData;
  reportForm = new FormGroup({
    violationType: new FormControl('', Validators.required),
    reportDetails: new FormControl('')
  });
  reportImageFile?: File;
  reportPhotoView?: string;

  goToDetail(id: number, type: number): void {
    if (type === 0) {
      this.router.navigate(['Board', 'detail', id]);
    }
    else if (type === 1) {
      this.router.navigate(['Board', 'JournalDetail', id]);
    }
  }

  goToMemderPage(memderID: string): void {
    this.router.navigate(['Board', 'user', memderID]);
  }
  animatingIds = new Set<number>();
  ToLike(id: number) {
    this.animatingIds.add(id);
    setTimeout(() => this.animatingIds.delete(id), 800);

    var article = this.articleList.find(a => a.articleId === id);
    if (article) {
      if (!article.isLike) {
        article.likeCount++;
        article.isLike = true;
      }
      else {
        article.likeCount--;
        article.isLike = false;
      }
      this.Serve.postArticleLike(id).subscribe();
    }
  }

  seachTag(tag: number) {
    console.log('seachTag emit', tag);
    this.tagSelected.emit(tag);
  }
  openReport(article: ArticleData) {
    this.reportTarget = article;
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
        TargetID: this.reportTarget.articleId,
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




