import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TestUse } from './test-use';

describe('TestUse', () => {
  let component: TestUse;
  let fixture: ComponentFixture<TestUse>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TestUse]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TestUse);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
