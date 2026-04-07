import { Component, Input, OnInit } from '@angular/core';
import { BoardServe } from '../../Service/board-serve';
import { CommentsDTO } from '../../interface/CommentsDTO';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { CloudinaryServe } from '../../Service/cloudinary-serve';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-comments-area',
  imports: [FormsModule],
  templateUrl: './comments-area.html',
  styleUrl: './comments-area.css',
})
export class CommentsArea implements OnInit {
  constructor(private _Cserve: CloudinaryServe, private Serve: BoardServe, private http: HttpClient,) {

  }
  ngOnInit(): void {
    this.ReflashComments();

  }
  cloudName = "daobwcaga"; // replace with your own cloud name
  uploadPreset = "ml_default"; // replace with your own upload preset
  @Input() id = 0;
  @Input() commentCount = 0;
  commentList: CommentsDTO[] = [];
  contents = "";
  parent?: number;
  parentComment? = "";
  selectedImage?: string = undefined;
  selectedImageFile?: File;
  ImageURL?: string;
  showPreview: boolean = false;

  async postComment(contents: string) {
    Swal.fire({
      title: "上傳中...",
      allowOutsideClick: false,
      didOpen: () => {
        Swal.showLoading();
      }
    });
    if (this.selectedImageFile) {
      this.ImageURL = await this._Cserve.uploadImage(this.selectedImageFile);
      console.log("上傳", this.parent, this.selectedImageFile);
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
        this.removeImage();
        this.commentCount++;
        Swal.fire({
          title: "留言已送出!",
          icon: "success",
          showConfirmButton: false,
          timer: 1000,
        });
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
    console.log("removeImage");
    this.selectedImageFile = undefined;
    this.selectedImage = undefined;
    this.showPreview = false;
  }

  deleteComment(id: number) {
    this.Serve.deleteComment(id).subscribe();
    this.commentList = this.commentList.filter(c => c.commentId !== id);
  }


  ToLike(id: number) {
    const comment = this.commentList.find(c => c.commentId === id);
    if (comment) comment.isLiked = !comment.isLiked;
  }



}


