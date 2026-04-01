import { ComponentFixture, TestBed } from '@angular/core/testing';

import { VersionCompareDialog } from './version-compare-dialog';

describe('VersionCompareDialog', () => {
  let component: VersionCompareDialog;
  let fixture: ComponentFixture<VersionCompareDialog>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VersionCompareDialog]
    })
    .compileComponents();

    fixture = TestBed.createComponent(VersionCompareDialog);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
