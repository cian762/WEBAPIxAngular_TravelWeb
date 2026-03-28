import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CreateArticleButton } from './create-article-button';

describe('CreateArticleButton', () => {
  let component: CreateArticleButton;
  let fixture: ComponentFixture<CreateArticleButton>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CreateArticleButton]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CreateArticleButton);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
