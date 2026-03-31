import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RouterMapComponent } from './router-map-component';

describe('RouterMapComponent', () => {
  let component: RouterMapComponent;
  let fixture: ComponentFixture<RouterMapComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RouterMapComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RouterMapComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
