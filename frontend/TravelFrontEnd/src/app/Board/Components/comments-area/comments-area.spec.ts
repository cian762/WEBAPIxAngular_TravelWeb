import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CommentsArea } from './comments-area';

describe('CommentsArea', () => {
  let component: CommentsArea;
  let fixture: ComponentFixture<CommentsArea>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CommentsArea]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CommentsArea);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
