import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CreatPost } from './creat-post';

describe('CreatPost', () => {
  let component: CreatPost;
  let fixture: ComponentFixture<CreatPost>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CreatPost]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CreatPost);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
