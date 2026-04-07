import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MyFollow } from './my-follow';

describe('MyFollow', () => {
  let component: MyFollow;
  let fixture: ComponentFixture<MyFollow>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MyFollow]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MyFollow);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
