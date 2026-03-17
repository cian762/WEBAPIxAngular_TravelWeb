import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Shoppingcart } from './shoppingcart';

describe('Shoppingcart', () => {
  let component: Shoppingcart;
  let fixture: ComponentFixture<Shoppingcart>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Shoppingcart]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Shoppingcart);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
