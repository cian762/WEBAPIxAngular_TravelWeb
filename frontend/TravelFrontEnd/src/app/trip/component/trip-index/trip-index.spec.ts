import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TripIndex } from './trip-index';

describe('TripIndex', () => {
  let component: TripIndex;
  let fixture: ComponentFixture<TripIndex>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TripIndex]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TripIndex);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
