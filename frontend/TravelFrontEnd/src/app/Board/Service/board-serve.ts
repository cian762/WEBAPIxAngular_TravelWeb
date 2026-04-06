import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ArticleData, ArticleResponse, TagDTO } from '../interface/ArticleData';
import { PostDetailDto } from '../interface/PostDetailDto';
import { switchMap } from 'rxjs/operators';
import { CommentsDTO } from '../interface/CommentsDTO';
import { environment } from '../../../environments/environment';
import { JournalElementDTO, JournalUpDateDTO } from '../interface/JournalElementDTO';
import { JournalDetailDTO } from '../interface/JournalDetailDTO';

@Injectable({
  providedIn: 'root',
})
export class BoardServe {
  constructor(private http: HttpClient) {

  }

  baseUrl: string = environment.apiBaseUrl;
  private apiUrl = this.baseUrl;

  getArtcleForVister() {
    return this.http.get<ArticleData[]>(`${this.apiUrl}/Board/Articles/Visitors`)
  }

  getArticleAPI(para: number) {
    return this.http.get<ArticleResponse>(`${this.apiUrl}/Board/Articles/Bypage/${para}`);
  }

  getActicleByTrending() {
    return this.http.get(`${this.apiUrl}/Board/Articles/trending`);
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

  getArticleByAllSearch(dto: any) {
    const params = new HttpParams({
      fromObject:
        Object.fromEntries(Object.entries(dto).filter(([_, v]) => v != null).map(([k, v]) => [k, String(v)]))
    });
    return this.http.get<ArticleResponse>(`${this.apiUrl}/Board/Articles/searchByAll`, { params });

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

  getJournalAPI(id: number) {
    return this.http.get<JournalUpDateDTO>(
      `${this.apiUrl}/Board/Journals/${id}`);
  }

  getJournalDetailAPI(id: number) {
    return this.http.get<JournalDetailDTO>(
      `${this.apiUrl}/Board/Journals/JournalDetail/${id}`);
  }

  postJournalAPI() {
    return this.http.post<number>(
      `${this.apiUrl}/Board/Journals`, null);
  }

  putJournalAPI(id: number, para: JournalUpDateDTO) {
    return this.http.put(
      `${this.apiUrl}/Board/Journals/${id}`, para);
  }

  getNewActivity() {
    return this.http.get(`${this.apiUrl}/ActivityInfo/NewActivity`)
  }

  getCurUser() {
    return this.http.get(`${this.apiUrl}/Board/Articles/curUser?page=1`)
  }
  getAuthorUser(para: string) {
    return this.http.get(`${this.apiUrl}/Board/Articles/authorUser?authorID=${para}`)
  }

  getAuthorUserInfo(para: string) {
    return this.http.get(`${this.apiUrl}/Board/Articles/getAuthorUserInfo?authorID=${para}`)
  }


  deleteArticle(id: number) {
    return this.http.delete(`${this.apiUrl}/Board/Articles/${id}`)
  }

  postFollow(id: string) {
    return this.http.post(`${this.apiUrl}/Follow/toggle`, {
      "followedId": id
    })
  }

  postReport(para: any) {
    return this.http.post(`${this.apiUrl}/Board/ReportLogs`, para)
  }



  getIsFollowing(id: string) {
    return this.http.get<boolean>(`${this.apiUrl}/Board/ArticlePermissions/isFollowing?followedId=${id}`
    )
  }

  postBlock(id: string) {
    return this.http.post(`${this.apiUrl}/Block/toggle`, {
      "blockedId": id,
      "reason": null
    })
  }

  deleteComment(id: number) {
    return this.http.delete(`${this.apiUrl}/Board/Comments/DeleteComment?commentID=${id}`)
  }

  postLogView(id: number) {
    return this.http.post(`${this.apiUrl}/Board/UserActivityLogs/${id}/view`, null)
  }

}



// getArticleCreatAPI(UserID: string) {
//   return this.http.get(`https://localhost:7276/api/Board/Articles?Type=0&UserId=${UserID}`);
// }
