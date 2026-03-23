import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AttractionService } from '../attraction.service';
import { AttractionType } from '../attraction.models';

@Component({
  selector: 'app-attraction-tags',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './attraction-tags.html',
  styleUrls: ['./attraction-tags.css']
})
export class AttractionTagsComponent implements OnInit {
  types: AttractionType[] = [];

  constructor(private svc: AttractionService, private router: Router) { }

  ngOnInit(): void {
    this.svc.getAttractionTypes().subscribe(t => {
      this.types = t;
    });
  }

  goToTag(type: AttractionType): void {
    this.router.navigate(['/attractions/list'], {
      queryParams: {
        typeId: type.attractionTypeId,
        cityName: type.attractionTypeName
      }
    });
  }
}
