import { TestBed } from '@angular/core/testing';

import { GoogleMAPservice } from './google-mapservice';

describe('GoogleMAPservice', () => {
  let service: GoogleMAPservice;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(GoogleMAPservice);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
