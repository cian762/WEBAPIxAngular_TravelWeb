import { Component, Input, OnInit } from '@angular/core';
import { BoardServe } from '../../Service/board-serve';
import { CommentsDTO } from '../../interface/CommentsDTO';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-comments-area',
  imports: [FormsModule],
  templateUrl: './comments-area.html',
  styleUrl: './comments-area.css',
})
export class CommentsArea implements OnInit {
  constructor(private Serve: BoardServe) {

  }
  ngOnInit(): void {
    this.ReflashComments();

  }

  @Input() id = 0;
  commentList: CommentsDTO[] = [];
  contents = "";
  parent?: number;
  parentComment? = "";
  selectedImageUrl = "";
  showPreview: boolean = false;

  postComment(contents: string) {
    var CommentDto = {
      articleID: this.id,
      contents: contents,
      parentID: this.parent !== undefined ? this.parent : null,
    }
    console.log(CommentDto);
    this.Serve.postCommentAPI(CommentDto).subscribe(({
      next: (res) => {
        this.ReflashComments();
        this.contents = '';
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
      const reader = new FileReader();
      reader.onload = () => {
        this.selectedImageUrl = reader.result as string;
      };
      reader.readAsDataURL(file);
    }
  }

}


