import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { paginationInterface } from '../Interface/paginationInterface';
import { queryParameters } from '../Component/info-card/info-card';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class CardInfoService {
  constructor(private http: HttpClient) { }
  baseUrl: string = environment.apiBaseUrl;

  FilterCardInfo(query: queryParameters) {

    let params = new HttpParams();
    query.type.forEach(r => { params = params.append('type', r) });
    query.region.forEach(r => { params = params.append('region', r) });
    params = params.set('start', query.start ?? '');
    params = params.set('end', query.end ?? '');
    params = params.set('pagenumber', query.pagenumber);
    params = params.set('orderbyparam', query.orderbyparam);
    params = params.set('pagesize', query.pagesize);
    params = params.set('keyword', query.keyword);

    return this.http.get<paginationInterface>(`${this.baseUrl}/ActivityCard/Query`, {
      params
    });
  }

  keywordSuggestion(keyword: string) {
    let params = new HttpParams();
    params = params.set('searchtext', keyword);
    return this.http.get<string[]>(`${this.baseUrl}/ActivityCard/Keyword`, { params });
  }
}
