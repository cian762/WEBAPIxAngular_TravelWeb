import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ArticleData, ArticleResponse, TagDTO } from '../interface/ArticleData';
import { PostDetailDto } from '../interface/PostDetailDto';
import { switchMap } from 'rxjs/operators';
import { CommentsDTO } from '../interface/CommentsDTO';

@Injectable({
  providedIn: 'root',
})
export class BoardServe {
  constructor(private http: HttpClient) {

  }
  private apiUrl = 'https://localhost:7276/api';
  getArticleAPI(para: number) {
    return this.http.get<ArticleResponse>(`${this.apiUrl}/Board/Articles/Bypage/${para}`);
  }

  getArticleByUserAPI(para: number) {
    return this.http.get<ArticleResponse>(`${this.apiUrl}/Board/Articles/articlesByUser?page=${para}`);
  }

  getArticleByKeyword(page: number, keyword: string) {
    return this.http.get<ArticleResponse>(`${this.apiUrl}/Board/Articles/search?page=${page}&keyword=${keyword}`);
  }

  getArticleByDate(page: number, startTime: Date, endTime: Date) {
    return this.http.get<ArticleResponse>(`${this.apiUrl}/Board/Articles/searchByDate?page=${page}&startTime=${startTime}&endTime=${endTime}`);
  }

  getArticleByAuthor(page: number, authorId: string) {
    return this.http.get<ArticleResponse>(`${this.apiUrl}/Board/Articles/searchByAuthor?page=${page}&authorID=${authorId}`)
  }

  getArticleByTags(page: number, isprecise: boolean, TagsId?: string[]) {
    return this.http.get<ArticleResponse>(`${this.apiUrl}/Board/Articles/searchByTags?page=${page}&${TagsId}&isprecise=false`)
  }

  getArticleByAllSearch(page: number, authorId?: string) {
    return this.http.get<ArticleResponse>(`${this.apiUrl}/Board/Articles/searchByAll?page=${page}&Keyword=10000006&StartTime=2026-02-21&EndTime=2026-02-28&AuthorId=${authorId}&TagIds=6`)
  }

  getArticleByCollect(page: number, authorId?: string) {
    return this.http.get<ArticleResponse>(`${this.apiUrl}/Board/Articles/articlesByCollect?page=1`)
  }

  getArticleDetailAPI(para: number) {
    return this.http.get<PostDetailDto>(`${this.apiUrl}/Post/${para}`);
  }

  getTagsByArticleAPI(para: number) {
    return this.http.get(`${this.apiUrl}/Board/Tags/articleId/${para}`)
  }
  getComments(para: number) {
    return this.http.get<CommentsDTO[]>(`${this.apiUrl}/Board/Comments/${para}`);
  }
  getAllTags() {
    return this.http.get<TagDTO[]>
      (`${this.apiUrl}/Board/Tags/all`)
  }

  getAllRegions() {
    return this.http.get(`${this.apiUrl}/Board/Articles/AllRegions`)
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

  postArticleLike(articleId: number) {
    return this.http.post(
      `${this.apiUrl}/Board/Articles/Like?articleID=${articleId}`, null)
  }

  postArticleCollect(articleId: number) {
    return this.http.post(
      `${this.apiUrl}/Board/Articles/Collect?articleID=${articleId}`, null)
  }

  putPostAPI(id: number, para: any) {
    return this.http.put(
      `${this.apiUrl}/Post/${id}`, para);
  }

  getNewActivity() {
    return this.http.get(`${this.apiUrl}/ActivityInfo/NewActivity`)
  }

  getCurUser() {
    return this.http.get(`${this.apiUrl}/Board/Articles/curUser?page=1`)
  }
  getAuthorUser(para: string) {
    return this.http.get(`${this.apiUrl}/Board/Articles/authorUser?userId=${para}`)
  }

  deleteArticle(id: number) {
    return this.http.delete(`${this.apiUrl}/Board/Articles/${id}`)
  }

}



// getArticleCreatAPI(UserID: string) {
//   return this.http.get(`https://localhost:7276/api/Board/Articles?Type=0&UserId=${UserID}`);
// }
