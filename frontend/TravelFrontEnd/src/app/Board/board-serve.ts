import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ArticleData } from './interface/ArticleData';
import { PostDetailDto } from './interface/PostDetailDto';
import { switchMap } from 'rxjs/operators';

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
    return this.http.get<PostDetailDto>(`https://localhost:7276/api/Post/${para}`);
  }

  postPostAPI(UserID: string) {
    return this.http.post<number>(`https://localhost:7276/api/Board/Articles?Type=0&UserId=${UserID}`, null).pipe(
      switchMap(result => this.http.post<number>(`https://localhost:7276/api/Post?id=${result}`, null)));
  }

  putPostAPI(id: number, para: any) {
    return this.http.put(`https://localhost:7276/api/Post/${id}`, para);
  }
}



// getArticleCreatAPI(UserID: string) {
//   return this.http.get(`https://localhost:7276/api/Board/Articles?Type=0&UserId=${UserID}`);
// }
