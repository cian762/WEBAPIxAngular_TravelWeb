import { Component } from '@angular/core';
import { ActivatedRoute, Route, Router } from '@angular/router';

@Component({
  selector: 'app-page404',
  imports: [],
  templateUrl: './page404.html',
  styleUrl: './page404.css',
})
export class Page404 {
  constructor(private router: Router) { }
  goBackHome() {
    this.router.navigate(['Board']);
  }
}
