import { Component, inject, OnInit, Pipe } from '@angular/core';
import { CardInfoService } from '../../Service/card-info-service';
import { cardInfoForIndex } from '../../Interface/cardInfoForIndex';
import { SlicePipe } from '@angular/common';
import { Router } from '@angular/router';


@Component({
  selector: 'app-activity-index-card',
  imports: [SlicePipe],
  templateUrl: './activity-index-card.html',
  styleUrl: './activity-index-card.css',
})
export class ActivityIndexCard implements OnInit {

  private activityService = inject(CardInfoService);
  private route = inject(Router);

  result: cardInfoForIndex[] = [];

  ngOnInit(): void {
    this.activityService.CardInfoForIndex().subscribe({
      next: (res) => {
        this.result = res;
      },
      error: (err) => {
        console.log(err);
      },
    });
  }


  onClick(activityId: number) {
    this.route.navigate(['ActivityInfo', activityId]);
  }

}

