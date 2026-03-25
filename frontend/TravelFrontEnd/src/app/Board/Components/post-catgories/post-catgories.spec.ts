import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PostCatgories } from './post-catgories';

describe('PostCatgories', () => {
  let component: PostCatgories;
  let fixture: ComponentFixture<PostCatgories>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PostCatgories]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PostCatgories);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
