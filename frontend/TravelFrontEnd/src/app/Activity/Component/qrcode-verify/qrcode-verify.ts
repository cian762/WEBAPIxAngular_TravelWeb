import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { QrcodeService } from '../../Service/qrcode-service';
import { ActivatedRoute } from '@angular/router';
import { DatePipe, NgClass } from '@angular/common';
import { Observable, Subject, switchMap, takeUntil, takeWhile, timer, filter, map } from 'rxjs';
import { qrCodeResponse } from '../../Interface/qrCodeResponse';

@Component({
  selector: 'app-qrcode-verify',
  imports: [NgClass, DatePipe],
  templateUrl: './qrcode-verify.html',
  styleUrl: './qrcode-verify.css',
})
export class QrcodeVerify implements OnInit, OnDestroy {
  token = '';
  role: string | null = '';

  result = {
    productCode: '',
    productName: '',
    message: '',
    expiredDate: null as Date | null,
    useAt: null as Date | null,
    qrcodeImage: ''
  };

  private qrCodeService = inject(QrcodeService);
  private route = inject(ActivatedRoute);
  private destroy$ = new Subject<void>();

  ngOnInit(): void {
    this.role = localStorage.getItem('role');

    this.route.params.pipe(
      map(params => params['token'] as string),
      filter(token => !!token),
      takeUntil(this.destroy$)
    ).subscribe(token => {
      this.token = token;

      if (this.role !== 'Admin') {
        timer(0, 5000).pipe(
          switchMap(() => this.verifyQRcode(this.token)),
          takeWhile(res => res.message === '票券可使用', true),
          takeUntil(this.destroy$)
        ).subscribe({
          next: (res) => this.updateResult(res),
          error: (err) => console.error('驗證 QRcode 時發生錯誤', err)
        });
      } else {
        this.verifyQRcode(this.token).subscribe({
          next: (res) => { this.updateResult(res); console.log(res); },
          error: (err) => console.error('驗證 QRcode 時發生錯誤', err)
        });
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  verifyQRcode(token: string): Observable<qrCodeResponse> {
    return this.qrCodeService.verifyQRcode(token);
  }

  redeemQRcode(token: string) {
    this.qrCodeService.RedeemQRcode(token).subscribe({
      next: (res) => this.updateResult(res),
      error: (err) => console.error('核銷 QRcode 時發生錯誤', err)
    });
  }

  private updateResult(res: qrCodeResponse): void {
    this.result.productCode = res.productCode ?? '';
    this.result.productName = res.productName ?? '';
    this.result.message = res.message ?? '';
    this.result.expiredDate = res.expiredDate ?? null;
    this.result.useAt = res.useAt ?? null;
    this.result.qrcodeImage = res.qrCodeImage
      ? 'data:image/png;base64,' + res.qrCodeImage
      : '';
    console.log('要停止打API了');
  }
}
