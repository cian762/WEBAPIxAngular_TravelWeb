import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TripProductDetail } from './trip-product-detail';

describe('TripProductDetail', () => {
  let component: TripProductDetail;
  let fixture: ComponentFixture<TripProductDetail>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TripProductDetail]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TripProductDetail);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
