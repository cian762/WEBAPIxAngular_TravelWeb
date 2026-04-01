import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ItineraryMetricscomponent } from './itinerary-metricscomponent';

describe('ItineraryMetricscomponent', () => {
  let component: ItineraryMetricscomponent;
  let fixture: ComponentFixture<ItineraryMetricscomponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ItineraryMetricscomponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ItineraryMetricscomponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
