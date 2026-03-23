import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ticketInfoInterface } from '../Interface/ticketInfoInterface';

@Injectable({
  providedIn: 'root',
})
export class TicketInfoService {
  constructor(private http: HttpClient) { }

  getTicketInfoService(productCode: string) {
    let params = new HttpParams();
    params = params.set('productCode', productCode)

    return this.http.get<ticketInfoInterface>('https://localhost:7276/api/ActivityTicket', { params });
  }

}

