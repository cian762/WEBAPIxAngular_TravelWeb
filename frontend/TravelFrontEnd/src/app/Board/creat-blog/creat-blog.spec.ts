import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CreatBlog } from './creat-blog';

describe('CreatBlog', () => {
  let component: CreatBlog;
  let fixture: ComponentFixture<CreatBlog>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CreatBlog]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CreatBlog);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
