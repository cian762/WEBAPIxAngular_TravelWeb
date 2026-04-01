import { TestBed } from '@angular/core/testing';

import { ItineraryAnalysis } from './itinerary-analysis';

describe('ItineraryAnalysis', () => {
  let service: ItineraryAnalysis;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ItineraryAnalysis);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
