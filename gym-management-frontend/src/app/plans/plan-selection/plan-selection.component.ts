import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { Plan } from '../../models/user.model';
import { PaymentComponent } from '../../payment/payment/payment.component';
import { MatDialog } from '@angular/material/dialog';
import { stringify } from 'node:querystring';

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
  userId!:Number;
  email:string='';
  isMember:boolean=false;
  isMemberOfGym:boolean=false;

  constructor(private http: HttpClient, private dialog: MatDialog, private router: Router) {}

  ngOnInit() {
    // Get gymId from route parameter (assuming route like /plans/:gymId)
    this.route.paramMap.subscribe(params => {
      this.gymId = Number(params.get('gymId'));
      this.userId=Number(params.get('userId'));
      this.email = params.get('email') || '';
      this.isMember=Boolean(params.get('isMember'));
      console.log('Selected Gym ID:', this.gymId);
      this.fetchPlans();
      this.checkUserMembership(this.userId,this.gymId);
    });
  }

  fetchPlans() {
    // Update URL to match your API endpoint for getting plans filtered by gymId
    this.http.get<Plan[]>(`https://localhost:7008/api/Payment/gym/${this.gymId}`)
      .subscribe((data) => {
        this.plans = data;
      }, error => {
        console.error('Error fetching plans:', error);
      });
  }

  checkUserMembership(userId: Number, gymId: number): void {
    this.http.get<boolean>(`https://localhost:7008/api/Member/isMember?userId=${userId}&gymId=${gymId}`)
      .subscribe(
        (isMember) => {
          console.log('User membership status:', isMember);
           this.isMemberOfGym=isMember;
        },
        (error) => {
          console.error('Error checking membership:', error);
        }
      );
  }

  goToPayment(plan: Plan) {
    this.router.navigate(['/payment'], {
      state: { plan: plan,
        userId: this.userId,
        isMember: this.isMember,
        email: this.email,
      } // passing selected plan via router state
    });
  }
}
