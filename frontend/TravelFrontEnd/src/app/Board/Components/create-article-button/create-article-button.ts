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

  }

  goToCreatePost(): void {
    this.Serve.postPostAPI().subscribe(p => {
      window.open(`/app/Board/creat/${p}`, '_blank');

    });
  }
  goToCreateJournal(): void {
    this.Serve.postJournalAPI().subscribe(p => {
      window.open(`/app/Board/creatJournal/${p}`, '_blank');

    });

  }


  goToCreateBlog(): void {
    window.open('/app/Board/creatBlog', '_blank');
    // this.Serve.postPostAPI().subscribe(p => {
    //   window.open('/app/Board/creatBlog', '_blank');
    // });
  }
}
