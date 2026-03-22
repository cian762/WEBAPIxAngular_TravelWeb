import { TestBed } from '@angular/core/testing';

import { ProductDetailPage } from './product-detail-page';

describe('ProductDetailPage', () => {
  let service: ProductDetailPage;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ProductDetailPage);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
