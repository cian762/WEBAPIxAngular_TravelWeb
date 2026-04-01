
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { routeResponseInterface } from '../Interface/routeResponseInterface';
import { Observable } from 'rxjs';
import { routeRequestInterface } from '../Interface/routeRequestInterface';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class RouteService {
  baseUrl: string = environment.apiBaseUrl;

  constructor(private http: HttpClient) { }

  getRoute(data: routeRequestInterface): Observable<routeResponseInterface> {

    return this.http.post<routeResponseInterface>(`${this.baseUrl}/ActivityInfo/RoutePlan`, data);
  }
}


