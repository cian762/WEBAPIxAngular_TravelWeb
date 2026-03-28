import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UserArticlesPage } from './user-articles-page';

describe('UserArticlesPage', () => {
  let component: UserArticlesPage;
  let fixture: ComponentFixture<UserArticlesPage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserArticlesPage]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UserArticlesPage);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
