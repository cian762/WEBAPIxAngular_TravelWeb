import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Travelindex } from './travelindex';

describe('Travelindex', () => {
  let component: Travelindex;
  let fixture: ComponentFixture<Travelindex>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Travelindex]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Travelindex);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
