import { TagDTO } from './../../interface/ArticleData';
import { Component, EventEmitter, OnInit, Output } from '@angular/core';
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
  @Output() tagSelected = new EventEmitter<number>();
  allTags: TagDTO[] = [];
  ngOnInit(): void {
    this.Serve.getAllTags().subscribe(d => this.allTags = d);
  }

  seachTag(tag: number) {
    this.tagSelected.emit(tag);
  }

}
