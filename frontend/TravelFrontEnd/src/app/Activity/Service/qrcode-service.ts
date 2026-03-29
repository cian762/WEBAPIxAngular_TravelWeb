import { ActivatedRoute } from '@angular/router';
import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { qrCodeResponse } from '../Interface/qrCodeResponse';

@Injectable({
  providedIn: 'root',
})
export class QrcodeService {
  baseUrl: string = "";
  token: string = "";
  private http = inject(HttpClient);

  verifyQRcode(token: string): Observable<qrCodeResponse> {
    return this.http.post<qrCodeResponse>('https://localhost:7276/api/QrCode/verify', { token });
  }

  RedeemQRcode(token: string) {
    return this.http.post<qrCodeResponse>('https://localhost:7276/api/QrCode/redeem', { token });
  }
}
