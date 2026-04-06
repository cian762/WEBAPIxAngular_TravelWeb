import { Component, OnInit } from '@angular/core';
import { BoardServe } from '../Service/board-serve';
import { ActivatedRoute, Router } from '@angular/router';
import { ArticleData } from '../interface/ArticleData';
import { ArticleList } from "../Components/article-list/article-list";
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-my-follow',
  imports: [ArticleList, FormsModule],
  templateUrl: './my-follow.html',
  styleUrl: './my-follow.css',
})
export class MyFollow implements OnInit {
  constructor(private Serve: BoardServe, private route: ActivatedRoute, private router: Router) { }
  articleList: ArticleData[] = [];
  authorList: any[] = [];
  carouselPage = 0;
  authorKeyword = '';
  filteredAuthors: any[] = [];

  ngOnInit(): void {
    this.Serve.getArticlesByFollowed().subscribe(d => this.articleList = d.articleList)
    this.Serve.getFollowed().subscribe((d: any) => this.authorList = d);
  }


  prevPage() {
    if (this.carouselPage > 0) this.carouselPage--;
  }

  nextPage() {
    if ((this.carouselPage + 1) * 5 < this.authorList.length) this.carouselPage++;
  }



  onAuthorSearch(Keyword: string) {
    console.log(Keyword, this.filteredAuthors);
    this.filteredAuthors = this.authorList.filter(a =>
      a.authorName.includes(Keyword)
    );
    this.carouselPage = 0;

    console.log('探險家喵'.includes('機')); // false
    console.log('機器獅'.includes('機')); // true
  }

  get visibleAuthors() {
    const list = this.authorKeyword ? this.filteredAuthors : this.authorList;
    const start = this.carouselPage * 5;
    return list.slice(start, start + 5);
  }

  goToMemderPage(memderID: string): void {
    this.router.navigate(['Board', 'user', memderID]);
  }
}
