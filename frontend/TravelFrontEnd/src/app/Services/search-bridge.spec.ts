import { TestBed } from '@angular/core/testing';

import { SearchBridge } from './search-bridge';

describe('SearchBridge', () => {
  let service: SearchBridge;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SearchBridge);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
