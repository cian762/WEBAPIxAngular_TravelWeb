import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PageNumberList } from './page-number-list';

describe('PageNumberList', () => {
  let component: PageNumberList;
  let fixture: ComponentFixture<PageNumberList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PageNumberList]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PageNumberList);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
