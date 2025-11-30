import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GymOwnerDashboardComponent } from './gym-owner-dashboard.component';

describe('GymOwnerDashboardComponent', () => {
  let component: GymOwnerDashboardComponent;
  let fixture: ComponentFixture<GymOwnerDashboardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GymOwnerDashboardComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(GymOwnerDashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
