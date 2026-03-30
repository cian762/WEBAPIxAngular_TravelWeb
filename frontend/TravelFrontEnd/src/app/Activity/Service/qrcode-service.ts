import { ActivatedRoute } from '@angular/router';
import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { qrCodeResponse } from '../Interface/qrCodeResponse';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class QrcodeService {
  baseUrl: string = environment.apiBaseUrl;

  token: string = "";
  private http = inject(HttpClient);

  verifyQRcode(token: string): Observable<qrCodeResponse> {
    return this.http.post<qrCodeResponse>(`${this.baseUrl}/QrCode/verify`, { token });
  }

  RedeemQRcode(token: string) {
    return this.http.post<qrCodeResponse>(`${this.baseUrl}/QrCode/redeem`, { token });
  }
}
