import { TestBed } from '@angular/core/testing';

import { PersonalCommentService } from './personal-comment-service';

describe('PersonalCommentService', () => {
  let service: PersonalCommentService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PersonalCommentService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
