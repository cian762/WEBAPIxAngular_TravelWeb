import { Component, signal } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';
import { TestUse } from "./Components/test-use/test-use";
import { BlogHome } from "./Board/blog-home/blog-home";
import { Header } from "./header/header";
import { Footer } from "./footer/footer";
import { Banner } from "./banner/banner";
import { PostDetail } from "./Board/post-detail/post-detail";
import { Shoppingcart } from './trip/component/shoppingcart/shoppingcart';
import { Order } from "./trip/component/order/order";
import { Product } from "./trip/component/product/product";
import { TripProductDetail } from "./trip/component/trip-product-detail/trip-product-detail";

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, TestUse, RouterLink, Shoppingcart, BlogHome, Header, Footer, Banner, PostDetail, Order, Product, TripProductDetail],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('TravelFrontEnd');
}
