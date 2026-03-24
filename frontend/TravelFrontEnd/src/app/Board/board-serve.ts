import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ArticleData } from './interface/ArticleData';
import { PostDetailDto } from './interface/PostDetailDto';
import { switchMap } from 'rxjs/operators';
import { CommentsDTO } from './interface/CommentsDTO';

@Injectable({
  providedIn: 'root',
})
export class BoardServe {
  constructor(private http: HttpClient) {

  }
  private apiUrl = 'https://localhost:7276/api';
  getArticleAPI(para: number) {
    return this.http.get(`${this.apiUrl}/Board/Articles/Bypage/${para}`);
  }

  getArticleByKeyword(page: number, keyword: string) {
    return this.http.get(`${this.apiUrl}/Board/Articles/search?page=${page}&keyword=${keyword}`);
  }

  getArticleByDate(page: number, startTime: Date, endTime: Date) {
    return this.http.get(`${this.apiUrl}/Board/Articles/searchByDate?page=${page}&startTime=${startTime}&endTime=${endTime}`);
  }

  getArticleDetailAPI(para: number) {
    return this.http.get<PostDetailDto>(`${this.apiUrl}/Post/${para}`);
  }

  getComments(para: number) {
    return this.http.get<CommentsDTO[]>(`${this.apiUrl}/Board/Comments/${para}`);
  }
  getAllTags() {
    return this.http.get
      (`${this.apiUrl}/Board/Tags/all`)
  }
  getTagsByArticleAPI(para: number) {
    return this.http.get(`${this.apiUrl}/articleId/${para}`)
  }

  postPostAPI() {
    return this.http.post<number>(`${this.apiUrl}/Board/Articles?Type=0`, null, { withCredentials: true }).pipe(
      switchMap(result => this.http.post<number>(`${this.apiUrl}/Post?id=${result}`, null)));
  }

  postCommentAPI(para: any) {
    return this.http.post
      (`${this.apiUrl}/Board/Comments/PostComment`, para, { withCredentials: true })
  }
  postTagsByArticleAPI(articleId: number, para: number[]) {
    return this.http.post(`${this.apiUrl}/Board/Tags?articleId=${articleId}`, para)
  }


  putPostAPI(id: number, para: any) {

    return this.http.put(
      `${this.apiUrl}/Post/${id}`, para);

  }


}



// getArticleCreatAPI(UserID: string) {
//   return this.http.get(`https://localhost:7276/api/Board/Articles?Type=0&UserId=${UserID}`);
// }
