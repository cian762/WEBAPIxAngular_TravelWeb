import { TestBed } from '@angular/core/testing';

import { CreateShoppingCart } from './create-shopping-cart';

describe('CreateShoppingCart', () => {
  let service: CreateShoppingCart;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CreateShoppingCart);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
