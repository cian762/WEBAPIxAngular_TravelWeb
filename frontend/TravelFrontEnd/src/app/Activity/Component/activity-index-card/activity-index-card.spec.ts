import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ActivityIndexCard } from './activity-index-card';

describe('ActivityIndexCard', () => {
  let component: ActivityIndexCard;
  let fixture: ComponentFixture<ActivityIndexCard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ActivityIndexCard]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ActivityIndexCard);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
