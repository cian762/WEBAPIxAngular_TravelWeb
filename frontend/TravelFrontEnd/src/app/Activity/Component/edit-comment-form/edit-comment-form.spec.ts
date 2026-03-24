import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditCommentForm } from './edit-comment-form';

describe('EditCommentForm', () => {
  let component: EditCommentForm;
  let fixture: ComponentFixture<EditCommentForm>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditCommentForm]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EditCommentForm);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
