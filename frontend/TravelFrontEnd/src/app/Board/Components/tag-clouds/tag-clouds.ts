import { TagDTO } from './../../interface/ArticleData';
import { Component, OnInit } from '@angular/core';
import { BoardServe } from '../../Service/board-serve';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-tag-clouds',
  imports: [],
  templateUrl: './tag-clouds.html',
  styleUrl: './tag-clouds.css',
})
export class TagClouds implements OnInit {
  constructor(private Serve: BoardServe, private route: ActivatedRoute, private router: Router) {
  }

  allTags: TagDTO[] = [];
  ngOnInit(): void {
    this.Serve.getAllTags().subscribe(d => this.allTags = d);
  }



}
