import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Plan } from '../../models/user.model';
import { HttpClient } from '@angular/common/http';
import { MemberService } from '../../services/member.service';
import { PaymentService } from '../../services/payment.service';
/* ... other imports ... */

@Component({
  selector: 'app-payment',
  imports: [CommonModule],
  templateUrl: './payment.component.html',
  styleUrls: ['./payment.component.css'],
  standalone: true,
})
export class PaymentComponent implements OnInit {
  selectedPlan?: Plan;
  userId!: number;
  isMember: boolean = false;
  isAdmin: boolean = false;
  email: string = '';
  loading = false;
  status: string = '';
  orderId: string = '';

  constructor(
    private router: Router,
    private route: ActivatedRoute, // <-- added
    private http: HttpClient,
    private memberService: MemberService,
    private paymentService: PaymentService
  ) {
    const nav = this.router.getCurrentNavigation();
    const state = nav?.extras?.state ?? history.state ?? {};
    this.selectedPlan = state['plan'];
    this.userId = state['userId'] ?? 0;
    this.isMember = state['isMember'] ?? false;
    this.email = state['email'] ?? '';
    this.status = state['status'] ?? '';
    this.orderId = state['orderId'] ?? '';
    this.isAdmin = state['isAdmin'] ?? false;
  }

  ngOnInit(): void {
    if (this.status === 'SUCCESS' && this.isMember == true) {
      // Auto-save member if we have required data
      if (this.userId && this.selectedPlan?.id) {
        // call saveMember
        this.saveMember(); // will handle UI/alerts
      } else {
        // if missing plan info, redirect user to payment page with message or show UI to pick plan
        console.warn(
          'Missing plan or userId when auto-saving member after payment'
        );
      }
    } else if (this.status === 'PENDING') {
      // show pending UI e.g. toast or local state
      console.log('Payment pending for order', this.orderId);
    } else if (this.status === 'FAILED') {
      console.log('Payment failed for order', this.orderId);
    }
  }

  makePayment() {
    if (!this.selectedPlan || !this.userId) {
      alert('Plan or user missing.');
      return;
    }

    this.loading = true;
    const payload = {
      userId: this.userId,
      planId: this.selectedPlan.id,
      amount: this.selectedPlan.price,
    };

    this.paymentService.createOrder(payload).subscribe({
      next: (res) => {
        // normalize session & order id
        const session = res.payment_session_id ?? res.sessionId ?? null;
        const orderId = res.order_id ?? res.orderId ?? null;
        const checkoutLink = res.checkoutLink ?? null;

        if (checkoutLink) {
          window.location.href = checkoutLink;
          return;
        }

        if (session) {
          // pass normalized fields to checkout page
          this.router.navigate(['/payment/checkout'], {
            state: {
              payment_session_id: session,
              order_id: orderId,
              paymentId: res.paymentId ?? null,
              amount: res.amount ?? payload.amount,
              currency: res.currency ?? 'INR',
              checkoutLink,
              userId: this.userId, // <- add this
              planId: this.selectedPlan?.id, // <- add this
              plan: this.selectedPlan,
            },
          });
          return;
        }

        alert('Order created but no session returned. Check server logs.');
      },
      error: (err) => {
        /* unchanged */
      },
    });
  }

  saveMember() {
    if (!this.userId) {
      alert('User id missing, cannot save member.');
      return;
    }
    const memberRequest = {
      userId: this.userId,
      planId: this.selectedPlan?.id,
      gymId: this.selectedPlan?.gymId,
      trainerId: this.selectedPlan?.trainerId || null,
    };

    this.memberService.addMember(memberRequest).subscribe({
      next: (res) => {
        alert('Member registered successfully!');
        this.router.navigate(['/home/member-dashboard'], {
          state: {
            email: this.email,
            userId: this.userId,
            isMember: this.isMember,
            isAdmin:this.isAdmin
          },
        });
      },
      error: (err) => {
        console.error(err);
        alert('Something went wrong while saving the member.');
      },
    });
  }

  cancelPayment() {
    history.back();
  }
}
