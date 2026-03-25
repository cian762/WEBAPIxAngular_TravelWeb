import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PopularPost } from './popular-post';

describe('PopularPost', () => {
  let component: PopularPost;
  let fixture: ComponentFixture<PopularPost>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PopularPost]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PopularPost);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
