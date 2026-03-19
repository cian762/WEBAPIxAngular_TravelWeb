import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ArticleData } from './interface/ArticleData';
import { PostDetailDto } from './interface/PostDetailDto';

@Injectable({
  providedIn: 'root',
})
export class BoardServe {
  constructor(private http: HttpClient) {

  }
  getArticleAPI() {
    return this.http.get<ArticleData[]>('https://localhost:7276/api/Board/Articles/test');
  }

  getArticleDetailAPI(para: number) {
    return this.http.get<PostDetailDto>('https://localhost:7276/api/Post/10000003');
  }

}
