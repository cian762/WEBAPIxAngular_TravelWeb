import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BoardBanner } from './board-banner';

describe('BoardBanner', () => {
  let component: BoardBanner;
  let fixture: ComponentFixture<BoardBanner>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BoardBanner]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BoardBanner);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
