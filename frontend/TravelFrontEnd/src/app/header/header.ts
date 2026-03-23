import { Component } from '@angular/core';
import { RouterLink } from "@angular/router";
import { RouterModule } from '@angular/router';


@Component({
  selector: 'app-header',
  imports: [RouterModule,RouterLink],
  templateUrl: './header.html',
  styleUrl: './header.css',
})
export class Header {

}
