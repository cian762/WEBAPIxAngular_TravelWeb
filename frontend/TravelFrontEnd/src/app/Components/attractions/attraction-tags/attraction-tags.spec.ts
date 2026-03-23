import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AttractionTags } from './attraction-tags';

describe('AttractionTags', () => {
  let component: AttractionTags;
  let fixture: ComponentFixture<AttractionTags>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AttractionTags]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AttractionTags);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
