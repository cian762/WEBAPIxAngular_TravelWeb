import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PersonalHomepage } from './personal-homepage';

describe('PersonalHomepage', () => {
  let component: PersonalHomepage;
  let fixture: ComponentFixture<PersonalHomepage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PersonalHomepage]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PersonalHomepage);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
