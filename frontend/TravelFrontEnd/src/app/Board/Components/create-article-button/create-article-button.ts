import { Component, OnInit } from '@angular/core';
import { BoardServe } from '../../Service/board-serve';
import { Router } from '@angular/router';

@Component({
  selector: 'app-create-article-button',
  imports: [],
  templateUrl: './create-article-button.html',
  styleUrl: './create-article-button.css',
})
export class CreateArticleButton implements OnInit {
  constructor(private Serve: BoardServe, private router: Router
  ) {

  }

  ngOnInit(): void {
    throw new Error('Method not implemented.');
  }

  goToCreatePost(): void {
    this.Serve.postPostAPI().subscribe(p => {
      window.open(`/app/Board/creat/${p}`, '_blank');
      // this.router.navigate(['Board', 'creat', p]);
    });
  }
  goToCreateJournal(): void {
    window.open(`/app/Board/creatJournal`, '_blank');
    // this.Serve.postPostAPI().subscribe(p => {
    //   // this.router.navigate(['Board', 'creat', p]);
    // });
  }


  goToCreateBlog(): void {
    window.open('/app/Board/creatBlog', '_blank');
    // this.Serve.postPostAPI().subscribe(p => {
    //   window.open('/app/Board/creatBlog', '_blank');
    // });
  }
}
