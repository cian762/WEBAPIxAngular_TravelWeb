import { TestBed } from '@angular/core/testing';

import { CloudinaryServe } from './cloudinary-serve';

describe('CloudinaryServe', () => {
  let service: CloudinaryServe;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CloudinaryServe);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
