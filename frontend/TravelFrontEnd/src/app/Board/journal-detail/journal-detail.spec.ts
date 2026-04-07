import { ComponentFixture, TestBed } from '@angular/core/testing';

import { JournalDetail } from './journal-detail';

describe('JournalDetail', () => {
  let component: JournalDetail;
  let fixture: ComponentFixture<JournalDetail>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [JournalDetail]
    })
    .compileComponents();

    fixture = TestBed.createComponent(JournalDetail);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
