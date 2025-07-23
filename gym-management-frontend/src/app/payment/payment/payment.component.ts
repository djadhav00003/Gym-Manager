import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { Plan } from '../../models/user.model';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-payment',
  imports: [CommonModule],
  templateUrl: './payment.component.html',
  styleUrls: ['./payment.component.css'],
  standalone: true
})
export class PaymentComponent {
  selectedPlan: Plan;
  userId!:Number;
  isMember: boolean = false;
  email:string='';

  constructor(private router: Router, private http: HttpClient) {
    const nav = this.router.getCurrentNavigation();
    this.selectedPlan = nav?.extras?.state?.['plan'];
    this.userId=nav?.extras?.state?.['userId'];
    this.isMember=nav?.extras?.state?.['isMember'];
    this.email=nav?.extras?.state?.['email'];

  }

  makePayment() {
    alert(`Payment of ₹${this.selectedPlan.price} successful for ${this.selectedPlan.planName}`);
    this.saveMember();
  }

  saveMember() {
    const memberRequest = {
      userId: this.userId,
      planId: this.selectedPlan.id,
      gymId: this.selectedPlan.gymId,
      trainerId: this.selectedPlan.trainerId || null
    };

    this.http.post('https://localhost:7008/api/Member', memberRequest).subscribe({
      next: res => {
        alert('Member registered successfully!');
        this.router.navigate(['/home/member-dashboard'], { queryParams: { email: this.email,userId: this.userId ,isMember:this.isMember } });
      },
      error: err => {
        console.error(err);
        alert('Something went wrong while saving the member.');
      }
    });
  }


  cancelPayment() {
    history.back(); // or use Router to go back
  }
}
