import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { Plan } from '../../models/user.model';
import { PaymentComponent } from '../../payment/payment/payment.component';
import { MatDialog } from '@angular/material/dialog';
import { stringify } from 'node:querystring';
import { PaymentService } from '../../services/payment.service';
import { MemberService } from '../../services/member.service';
import { PlanService } from '../../services/plan.service';

@Component({
  selector: 'app-plan-selection',
  standalone: true,
  imports: [CommonModule,],
  templateUrl: './plan-selection.component.html',
  styleUrls: ['./plan-selection.component.css']
})
export class PlanSelectionComponent implements OnInit {
  route = inject(ActivatedRoute);
  gymId: number = 0;
  plans: Plan[] = [];
  selectedPlan: Plan | null = null;
  userId!:number;
  email:string='';
  isMember:boolean=false;
  isAdmin:boolean=false;
  isMemberOfGym:boolean=false;

  constructor(private http: HttpClient, private dialog: MatDialog, private router: Router,private planService: PlanService,
    private memberService:MemberService
  ) {}

  ngOnInit() {
    // Get gymId from route parameter (assuming route like /plans/:gymId)
    const state = history.state || {};
    if (state) {
       this.gymId   = Number(state['gymId']);
    this.userId  = Number(state['userId']);
    this.email   = String(state['email']);
    this.isMember= state['isMember'] === true || state['isMember'] === 'true';
    this.isMember= state['isAdmin'] === true || state['isAdmin'] === 'true';
    }
      this.fetchPlans();
      this.checkUserMembership(this.userId,this.gymId);

  }

  fetchPlans() {
    // Update URL to match your API endpoint for getting plans filtered by gymId
    this.planService.getPlansByGym(this.gymId).subscribe({
    next: (data) => this.plans = data,
    error: (err) => {
      console.error('Error fetching plans:', err);
      alert('Failed to load plans.');
    }
  });
  }


  checkUserMembership(userId: number, gymId: number): void {
  this.memberService.isUserMember(userId, gymId).subscribe({
    next: (isMember) => {
      console.log('User membership status:', isMember);
      this.isMemberOfGym = isMember;
    },
    error: (err) => {
      console.error('Error checking membership:', err);
    }
  });
  }

  goToPayment(plan: Plan) {
    this.router.navigate(['/payment'], {
      state: { plan: plan,
        userId: this.userId,
        isMember: this.isMember,
        email: this.email,
        isAdmin:this.isAdmin
      } // passing selected plan via router state
    });
  }
}
