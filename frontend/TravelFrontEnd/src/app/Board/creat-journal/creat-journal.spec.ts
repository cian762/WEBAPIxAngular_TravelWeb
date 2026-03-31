import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CreatJournal } from './creat-journal';

describe('CreatJournal', () => {
  let component: CreatJournal;
  let fixture: ComponentFixture<CreatJournal>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CreatJournal]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CreatJournal);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
