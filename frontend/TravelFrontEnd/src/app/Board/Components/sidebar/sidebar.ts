import { Component, Input, SimpleChanges } from '@angular/core';
import { PostDetailDto } from '../../interface/PostDetailDto';

@Component({
  selector: '[app-sidebar]',
  imports: [],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css',
})
export class Sidebar {
  @Input() post?: PostDetailDto;

}
