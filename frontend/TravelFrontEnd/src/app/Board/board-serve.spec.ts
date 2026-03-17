import { TestBed } from '@angular/core/testing';

import { BoardServe } from './board-serve';

describe('BoardServe', () => {
  let service: BoardServe;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(BoardServe);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
