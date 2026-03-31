import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CenterEditor } from './center-editor';

describe('CenterEditor', () => {
  let component: CenterEditor;
  let fixture: ComponentFixture<CenterEditor>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CenterEditor]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CenterEditor);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
