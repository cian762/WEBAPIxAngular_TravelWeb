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
    const params = new HttpParams()
      .set('type', query.type.toString())
      .set('region', query.region.toString())
      .set('start', query.start ?? '')
      .set('end', query.end ?? '');
    return this.http.get<paginationInterface>('https://localhost:7276/api/ActivityCard/Query', {
      params
    });
  }
}
