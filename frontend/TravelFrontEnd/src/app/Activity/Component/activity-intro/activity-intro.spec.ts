import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ActivityIntro } from './activity-intro';

describe('ActivityIntro', () => {
  let component: ActivityIntro;
  let fixture: ComponentFixture<ActivityIntro>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ActivityIntro]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ActivityIntro);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
