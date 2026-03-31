import { Component, OnInit } from '@angular/core';
import { BoardServe } from '../../Service/board-serve';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-post-catgories',
  imports: [],
  templateUrl: './post-catgories.html',
  styleUrl: './post-catgories.css',
})
export class PostCatgories implements OnInit {

  constructor(private Serve: BoardServe, private route: ActivatedRoute, private router: Router) {

  }
  newActivityList: any[] = []
  ngOnInit(): void {
    this.Serve.getNewActivity().subscribe((d: any) => {
      this.newActivityList = d;
      console.log("newActivityList" + this.newActivityList);
    });
  }

}
