import { TestBed } from '@angular/core/testing';
import { CanActivateFn } from '@angular/router';

import { aGuard } from './a-guard';

describe('aGuard', () => {
  const executeGuard: CanActivateFn = (...guardParameters) => 
      TestBed.runInInjectionContext(() => aGuard(...guardParameters));

  beforeEach(() => {
    TestBed.configureTestingModule({});
  });

  it('should be created', () => {
    expect(executeGuard).toBeTruthy();
  });
});
