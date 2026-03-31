import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Itinerarylist } from './itinerarylist';

describe('Itinerarylist', () => {
  let component: Itinerarylist;
  let fixture: ComponentFixture<Itinerarylist>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Itinerarylist]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Itinerarylist);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
