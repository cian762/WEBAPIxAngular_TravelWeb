import { Component, Input, OnInit } from '@angular/core';
import { BoardServe } from '../../Service/board-serve';
import { CommentsDTO } from '../../interface/CommentsDTO';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-comments-area',
  imports: [FormsModule],
  templateUrl: './comments-area.html',
  styleUrl: './comments-area.css',
})
export class CommentsArea implements OnInit {
  constructor(private Serve: BoardServe, private http: HttpClient,) {

  }
  ngOnInit(): void {
    this.ReflashComments();

  }
  cloudName = "daobwcaga"; // replace with your own cloud name
  uploadPreset = "ml_default"; // replace with your own upload preset
  @Input() id = 0;
  commentList: CommentsDTO[] = [];
  contents = "";
  parent?: number;
  parentComment? = "";
  selectedImage = "";
  selectedImageFile?: File;
  ImageURL?: string;
  showPreview: boolean = false;

  async postComment(contents: string) {
    if (this.selectedImageFile) {
      this.ImageURL = await this.uploadImage(this.selectedImageFile);
    }

    var CommentDto = {
      articleID: this.id,
      contents: contents,
      parentID: this.parent !== undefined ? this.parent : null,
      commentPhoto: this.ImageURL
    }
    console.log(CommentDto);
    this.Serve.postCommentAPI(CommentDto).subscribe(({
      next: (res) => {
        this.ReflashComments();
        this.contents = '';
        this.removeParent();
        this.removeImage;

      },
      error: (err) => {
      }
    }))

  }

  setParent(commentID: number, authorName: string, contents: string) {
    this.parent = commentID;
    this.parentComment = ("TO" + authorName + ":" + contents);
    console.log(this.parent);
  }

  removeParent() {
    this.parent = undefined;
    this.parentComment = "";
  }

  ReflashComments() {
    this.Serve.getComments(this.id).subscribe((d) => {
      this.commentList = d;
    });
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      this.selectedImageFile = file; // 存起來
      const reader = new FileReader();
      reader.onload = () => {
        this.selectedImage = reader.result as string;
      };
      reader.readAsDataURL(file);
    }
  }

  removeImage() {
    this.selectedImage = "";
    this.showPreview = false;
  }




  async uploadImage(file: File) {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('upload_preset', this.uploadPreset);

    const url = `https://api.cloudinary.com/v1_1/${this.cloudName}/image/upload`;

    try {
      // 使用 firstValueFrom 將 Observable 轉為 Promise
      // 程式會在這裡停住，直到拿到 API 回傳值
      const res = await firstValueFrom(this.http.post<any>(url, formData));

      return res.secure_url;
    } catch (err) {
      console.error('上傳失敗', err);
      throw err;
    }
  }




}


