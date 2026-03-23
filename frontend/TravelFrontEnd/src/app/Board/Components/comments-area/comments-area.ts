import { Component, Input, OnInit } from '@angular/core';
import { BoardServe } from '../../board-serve';
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
  UserId = "";
  contents = "";
  parent?: number;
  parentComment? = "";

  postComment(UserId: string, contents: string) {
    var CommentDto = {
      articleID: this.id,
      userId: UserId,
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

  ReflashComments() {
    this.Serve.getComments(this.id).subscribe((d) => {
      this.commentList = d;
    });
  }

}


