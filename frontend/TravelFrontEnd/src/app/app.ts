import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { TestUse } from "./Components/test-use/test-use";
import { BlogHome } from "./Board/blog-home/blog-home";
import { Header } from "./header/header";
import { Footer } from "./footer/footer";
import { Banner } from "./banner/banner";
import { PostDetail } from "./Board/post-detail/post-detail";

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, TestUse, BlogHome, Header, Footer, Banner, PostDetail],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('TravelFrontEnd');
}
