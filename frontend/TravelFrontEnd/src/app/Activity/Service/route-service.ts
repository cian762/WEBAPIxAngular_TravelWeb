
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { routeResponseInterface } from '../Interface/routeResponseInterface';
import { Observable } from 'rxjs';
import { routeRequestInterface } from '../Interface/routeRequestInterface';

@Injectable({
  providedIn: 'root',
})
export class RouteService {
  private baseUrl = "https://localhost:7276/api/ActivityInfo/RoutePlan"

  constructor(private http: HttpClient) { }

  getRoute(data: routeRequestInterface): Observable<routeResponseInterface> {

    return this.http.post<routeResponseInterface>(this.baseUrl, data);
  }
}


