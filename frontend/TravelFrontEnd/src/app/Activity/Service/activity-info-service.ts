import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ActivityInfoInterface } from '../Interface/InfoInterface';
import { reviewsPackage } from '../Interface/reviewsPackage';
import { productInfoInterface } from '../Interface/productIntroInterface';
import { SuggestionInfo } from '../Component/activity-intro/activity-intro';
import { CardInfoModel } from '../Interface/cardInterface';

@Injectable({
  providedIn: 'root',
})
export class ActivityInfoService {

  constructor(private http: HttpClient) { }

  getActivityDetails(activityId: number) {

    return this.http.get<ActivityInfoInterface>("https://localhost:7276/api/ActivityInfo/" + activityId);
  }

  getRelatedReviews(activityId: number, orderRule: string = "highest") {

    let params = new HttpParams();
    params = params.set('orderRule', orderRule);
    return this.http.get<reviewsPackage>(`https://localhost:7276/api/ActivityInfo/${activityId}/Reviews`, { params });
  }

  getRelatedTickets(activityId: number) {

    return this.http.get<productInfoInterface[]>(`https://localhost:7276/api/ActivityInfo/${activityId}/Tickets`);
  }

  offerRelatedOptions(activityInfo: SuggestionInfo) {
    if (!activityInfo) return;

    let params = new HttpParams();
    params = params.set('activityId', activityInfo.activityId);
    activityInfo.activityType.forEach(t => { params = params.append('activityType', t) });
    console.log(params);

    return this.http.get<CardInfoModel[]>('https://localhost:7276/api/ActivityTicket/Suggest', { params });
  }

}


