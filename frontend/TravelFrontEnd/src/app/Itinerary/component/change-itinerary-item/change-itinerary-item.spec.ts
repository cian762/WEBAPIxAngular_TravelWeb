import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ChangeItineraryItem } from './change-itinerary-item';

describe('ChangeItineraryItem', () => {
  let component: ChangeItineraryItem;
  let fixture: ComponentFixture<ChangeItineraryItem>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ChangeItineraryItem]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ChangeItineraryItem);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
