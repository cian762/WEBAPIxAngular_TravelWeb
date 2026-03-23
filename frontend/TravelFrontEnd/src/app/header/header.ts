import { Component } from '@angular/core';
//YJ新增import { RouterLink }
import { RouterLink } from '@angular/router';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-header',
  imports: [RouterModule,RouterLink],
  templateUrl: './header.html',
  styleUrl: './header.css',
})
export class Header {

}
