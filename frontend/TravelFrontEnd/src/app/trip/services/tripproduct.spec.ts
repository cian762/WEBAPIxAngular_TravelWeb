import { TestBed } from '@angular/core/testing';

import { Tripproduct } from './tripproduct';

describe('Tripproduct', () => {
  let service: Tripproduct;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Tripproduct);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
