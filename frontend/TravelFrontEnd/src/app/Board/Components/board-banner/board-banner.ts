import { map } from 'rxjs';
import { Component } from '@angular/core';
import { BoardServe } from '../../Service/board-serve';


@Component({
  selector: 'app-board-banner',
  imports: [],
  templateUrl: './board-banner.html',
  styleUrl: './board-banner.css',
})
export class BoardBanner {
  constructor(private Serve: BoardServe) { };
  ForBanner?: string;
  ngOnInit(): void {
    const el = document.getElementById('bannerCarousel');
    new (window as any).bootstrap.Carousel(el, {
      interval: 3000,
      ride: true
    });
    this.Serve.getActicleByTrending().subscribe(
      (d: any) => {
        this.ForBanner = d.map((item: any) => item.photoUrl);
      }
    )
  };

  // ngAfterViewInit() {
  //   const el = document.getElementById('bannerCarousel');
  //   if (el) {
  //     new (window as any).bootstrap.Carousel(el, {
  //       interval: 1000,
  //       ride: 'carousel'
  //     });
  //   }
  // }
}
