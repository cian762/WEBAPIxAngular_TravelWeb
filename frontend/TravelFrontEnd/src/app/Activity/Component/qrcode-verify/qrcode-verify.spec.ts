import { ComponentFixture, TestBed } from '@angular/core/testing';

import { QrcodeVerify } from './qrcode-verify';

describe('QrcodeVerify', () => {
  let component: QrcodeVerify;
  let fixture: ComponentFixture<QrcodeVerify>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [QrcodeVerify]
    })
    .compileComponents();

    fixture = TestBed.createComponent(QrcodeVerify);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
