import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ActivityInfoInterface } from '../Interface/InfoInterface';
import { reviewsPackage } from '../Interface/reviewsPackage';
import { productInfoInterface } from '../Interface/productIntroInterface';
import { SuggestionInfo } from '../Component/activity-intro/activity-intro';
import { CardInfoModel } from '../Interface/cardInterface';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class ActivityInfoService {

  constructor(private http: HttpClient) { }

  baseUrl: string = environment.apiBaseUrl;

  getActivityDetails(activityId: number) {

    return this.http.get<ActivityInfoInterface>(`${this.baseUrl}/ActivityInfo/${activityId}`);
  }

  getRelatedReviews(activityId: number, orderRule: string = "highest") {

    let params = new HttpParams();
    params = params.set('orderRule', orderRule);
    return this.http.get<reviewsPackage>(`${this.baseUrl}/ActivityInfo/${activityId}/Reviews`, { params });
  }

  getRelatedTickets(activityId: number) {

    return this.http.get<productInfoInterface[]>(`${this.baseUrl}/ActivityInfo/${activityId}/Tickets`);
  }

  offerRelatedOptions(activityInfo: SuggestionInfo) {
    if (!activityInfo) return;

    let params = new HttpParams();
    params = params.set('activityId', activityInfo.activityId);
    activityInfo.activityType.forEach(t => { params = params.append('activityType', t) });
    console.log(params);

    return this.http.get<CardInfoModel[]>(`${this.baseUrl}/ActivityTicket/Suggest`, { params });
  }

}


