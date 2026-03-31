import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ticketInfoInterface } from '../Interface/ticketInfoInterface';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class TicketInfoService {
  constructor(private http: HttpClient) { }
  baseUrl: string = environment.apiBaseUrl;
  getTicketInfoService(productCode: string) {
    let params = new HttpParams();
    params = params.set('productCode', productCode)
    return this.http.get<ticketInfoInterface>(`${this.baseUrl}/ActivityTicket`, { params });
  }
}

