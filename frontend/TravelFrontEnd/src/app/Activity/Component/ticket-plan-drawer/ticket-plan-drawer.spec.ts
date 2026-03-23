import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TicketPlanDrawer } from './ticket-plan-drawer';

describe('TicketPlanDrawer', () => {
  let component: TicketPlanDrawer;
  let fixture: ComponentFixture<TicketPlanDrawer>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TicketPlanDrawer]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TicketPlanDrawer);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
