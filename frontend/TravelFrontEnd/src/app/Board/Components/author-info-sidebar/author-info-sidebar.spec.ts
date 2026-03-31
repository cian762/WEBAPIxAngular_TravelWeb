import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AuthorInfoSidebar } from './author-info-sidebar';

describe('AuthorInfoSidebar', () => {
  let component: AuthorInfoSidebar;
  let fixture: ComponentFixture<AuthorInfoSidebar>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AuthorInfoSidebar]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AuthorInfoSidebar);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
