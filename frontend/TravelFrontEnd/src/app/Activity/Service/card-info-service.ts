import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { paginationInterface } from '../Interface/paginationInterface';

@Injectable({
  providedIn: 'root',
})
export class CardInfoService {
  constructor(private http: HttpClient) { }

  getCardInfo() {
    return this.http.get<paginationInterface>('https://localhost:7276/api/ActivityCard');
  }
}
