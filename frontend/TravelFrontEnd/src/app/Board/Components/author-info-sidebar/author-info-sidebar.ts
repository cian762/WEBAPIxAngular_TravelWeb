import { Component, Input } from '@angular/core';
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
}
