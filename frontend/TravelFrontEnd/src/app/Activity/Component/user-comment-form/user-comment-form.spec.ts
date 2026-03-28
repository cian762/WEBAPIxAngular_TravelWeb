import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UserCommentForm } from './user-comment-form';

describe('UserCommentForm', () => {
  let component: UserCommentForm;
  let fixture: ComponentFixture<UserCommentForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserCommentForm]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UserCommentForm);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
