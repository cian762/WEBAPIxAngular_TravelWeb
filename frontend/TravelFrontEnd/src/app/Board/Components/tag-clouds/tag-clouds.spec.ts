import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TagClouds } from './tag-clouds';

describe('TagClouds', () => {
  let component: TagClouds;
  let fixture: ComponentFixture<TagClouds>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TagClouds]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TagClouds);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
