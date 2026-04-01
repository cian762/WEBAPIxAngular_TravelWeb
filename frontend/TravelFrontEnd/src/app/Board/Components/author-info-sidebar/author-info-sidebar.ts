import { Component, Input, OnInit, SimpleChanges } from '@angular/core';
import { BoardServe } from '../../Service/board-serve';
import { ActivatedRoute, Router } from '@angular/router';
import { PostDetailDto } from '../../interface/PostDetailDto';


@Component({
  selector: 'app-author-info-sidebar',
  imports: [],
  templateUrl: './author-info-sidebar.html',
  styleUrl: './author-info-sidebar.css',
})
export class AuthorInfoSidebar {
  constructor(private Serve: BoardServe, private route: ActivatedRoute, private router: Router) { }
  @Input() post?: PostDetailDto;
  authorID: string = "";
  isFollowing = false;
  // ngOnInit(): void {
  //   if (this.post?.authorID)
  //     this.authorID = this.post?.authorID;
  //   this.Serve.getIsFollowing(this.authorID).subscribe(d => this.isFollowing = d);
  // }
  ngOnChanges(changes: SimpleChanges): void {
    if (changes['post']?.currentValue?.authorID) {
      this.authorID = changes['post'].currentValue.authorID;
      this.Serve.getIsFollowing(this.authorID).subscribe(d => this.isFollowing = d);
    }
  }


  ToFollow() {
    if (this.authorID)
      this.Serve.postFollow(this.authorID).subscribe({
        next: (res: any) => {
          this.isFollowing = !this.isFollowing;
        },
        error: (err) => {
          console.error(err);
        }
      });
  }

  goToMemderPage(memderID: string): void {
    this.router.navigate(['Board', 'user', memderID]);
  }

  toBlock(memderID?: string) {
    if (memderID)
      this.Serve.postBlock(memderID).subscribe();
  }

}
