import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { paginationInterface } from '../Interface/paginationInterface';
import { queryParameters } from '../Component/info-card/info-card';

@Injectable({
  providedIn: 'root',
})
export class CardInfoService {
  constructor(private http: HttpClient) { }

  getCardInfo() {
    return this.http.get<paginationInterface>('https://localhost:7276/api/ActivityCard');
  }

  FilterCardInfo(query: queryParameters) {

    let params = new HttpParams();
    query.type.forEach(r => { params = params.append('type', r) });
    query.region.forEach(r => { params = params.append('region', r) });
    params = params.set('start', query.start ?? '');
    params = params.set('end', query.end ?? '');
    params = params.set('pagenumber', query.pagenumber);
    params = params.set('orderbypopularity', query.orderbypopularity);
    params = params.set('islatest', query.islatest);
    params = params.set('isobsolete', query.isobsolete);
    params = params.set('pagesize', query.pagesize);

    return this.http.get<paginationInterface>('https://localhost:7276/api/ActivityCard/Query', {
      params
    });
  }



}
