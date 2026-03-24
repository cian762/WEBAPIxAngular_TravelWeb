import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TicketSection } from './ticket-section';

describe('TicketSection', () => {
  let component: TicketSection;
  let fixture: ComponentFixture<TicketSection>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TicketSection]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TicketSection);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
