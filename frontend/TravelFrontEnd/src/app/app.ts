import { Component, signal } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';
import { TestUse } from "./Components/test-use/test-use";
import { Shoppingcart } from './trip/component/shoppingcart/shoppingcart';

@Component({
  selector: 'app-root', 
  imports: [RouterOutlet, TestUse, RouterLink,Shoppingcart],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('TravelFrontEnd');
}
