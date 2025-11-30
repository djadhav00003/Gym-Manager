import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { load } from '@cashfreepayments/cashfree-js';
import { firstValueFrom, Subscription } from 'rxjs';
import { PaymentService } from '../../services/payment.service';
import { MemberService } from '../../services/member.service';

@Component({
  selector: 'app-payment-checkout',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './payment-checkout.component.html',
  styleUrls: ['./payment-checkout.component.css'],
})
export class PaymentCheckoutComponent implements OnInit, OnDestroy {
  sessionId: string | null = null;
  orderId: string | null = null;
  paymentId: number | null = null;
  checkoutLink: string | null = null;
  loading = false;
  error: string | null = null;

  // NEW: user & plan info passed from PaymentComponent
  userId: number | null = null;
  planId: number | null = null;
  gymId: number | null = null;
  trainerId: number | null = null;
  plan: any = null;
  status: string='';
  isMember: boolean = false;
  isAdmin: boolean = false;


  private readonly MODE: 'sandbox' | 'production' = 'sandbox';
  private inited = false;
  private subs: Subscription[] = [];

  constructor(
    private router: Router,
    private paymentService: PaymentService,
    private memberService: MemberService // inject member service
  ) {}

  async ngOnInit(): Promise<void> {
    if (this.inited) return;
    this.inited = true;

    // query params (return from hosted checkout)
    const q = new URLSearchParams(window.location.search);
    const orderIdFromQuery = q.get('order_id') ?? q.get('orderId');
    const paymentSessionFromQuery =
      q.get('payment_session_id') ??
      q.get('paymentSessionId') ??
      q.get('paymentSession');


    // CASE A: returned after hosted checkout w/ order_id -> verify and finish
    if (orderIdFromQuery) {
      this.orderId = orderIdFromQuery;
      await this.handleRedirectReturn(orderIdFromQuery);
      return;
    }

    // CASE B: query supplied a payment session id
    if (paymentSessionFromQuery) {
      this.sessionId = paymentSessionFromQuery;
      const maybeOrderId = q.get('orderId') ?? q.get('order_id');
      if (maybeOrderId) this.orderId = maybeOrderId;
      await this.openCheckoutWithSdk();
      return;
    }

    // CASE C: normal navigation state from PaymentComponent
    const state = history.state ?? {};
    this.sessionId =
      state.payment_session_id ?? state.sessionId ?? state.sessionId ?? null;
    this.orderId = state.order_id ?? state.orderId ?? null;
    this.checkoutLink = state.checkoutLink ?? null;
    // Capture user/plan so we can call addMember after success
    this.userId = state.userId ?? null;
    this.planId = state.planId ?? null;
    this.plan = state.plan ?? null;
    this.paymentId = state.paymentId ?? null;

    if (!this.sessionId && !this.checkoutLink) {
      this.error =
        'Missing sessionId (payment session) and no hosted checkout link available.';
      return;
    }

    if (this.checkoutLink) {
      window.location.href = this.checkoutLink;
      return;
    }

    await this.openCheckoutWithSdk();
  }

  private async handleRedirectReturn(orderId: string) {
    this.loading = true;
    this.error = null;

    try {
      // verify with backend
      const res: any = await firstValueFrom(
        this.paymentService.verifyOrder(orderId)
      );
      this.loading = false;

      this.status = (res?.status ?? '').toString().toUpperCase();
      this.sessionId=res?.payment_session_id;
      if (this.status === 'SUCCESS' || this.status === 'PAID' || this.status === 'COMPLETED') {
        // Payment succeeded — now make the user a member (if userId & planId available)
         this.userId = res?.userId ?? null;
        this.planId = res?.planId ?? null;
        this.gymId = res?.gymId ?? null;
        this.trainerId = res?.trainerId ?? null;
        this.isMember=res?.isMember;
        this.isAdmin=res?.isAdmin;
        if (this.userId && this.planId &&  this.gymId ) {
            // call member API to register membership
             this.plan = {
              id: this.planId,
              gymId:  this.gymId,
              trainerId: this.trainerId,
            };
          try {
            this.router.navigate(['/payment'], {
              state: {
                 plan: this.plan,
                status: this.status,                 // canonical status
              orderId: orderId,                  // use orderId variable returned by server
              userId: this.userId,               // user id (number)
              isMember: this.isMember,
              isAdmin:this.isAdmin
     }
            });
            return;
          } catch (memberErr) {
            console.error('Member save failed:', memberErr);
            // Payment was still successful — but member creation failed. Show UI to retry.
            alert(
              'Payment succeeded but saving membership failed. Please try saving membership again from Payments page.'
            );
            this.router.navigate(['/payment'], {
                  state: {
                plan: this.plan,
                status: this.status,                 // canonical status
              orderId: orderId,                  // use orderId variable returned by server
              userId: this.userId,               // user id (number)
              isMember: true,
     }
            });
            return;
          }
        }
      }
      else if (this.status === 'PENDING' ||this.status === 'ACTIVE' ||this.status === 'CREATED') {
        alert('Payment pending. We will confirm shortly.');
        // Send the user back to payment page where they can see pending state and retry
        // this.router.navigate(['/payment'], {
        //   queryParams: { orderId, status: 'PENDING' },
        // });
      } else {
        alert('Payment failed or cancelled.');
        // this.router.navigate(['/payment'], {
        //   queryParams: { orderId, status: 'FAILED' },
        // });
      }
    } catch (err) {
      console.error('verifyOrder failed', err);
      this.loading = false;
      this.error = 'Could not verify payment. Try again later.';
    }
  }

  private async openCheckoutWithSdk() {
    this.loading = true;
    this.error = null;

    try {
      const cf = await load({ mode: this.MODE });

      if (!cf || typeof cf.checkout !== 'function') {
        this.loading = false;
        this.error =
          'Cashfree SDK loaded but checkout() not available on the returned object.';
        console.error('CF SDK object:', cf);
        return;
      }

 const options: any = {
      paymentSessionId: this.sessionId,
      // force redirect so we reliably get order_id query param and verify on server
      redirectTarget: '_self',

      // Fallback callbacks only — do not perform critical navigation here
      onSuccess: (payload: any) => {
        console.log('CF onSuccess (fallback):', payload);
        // optional: show small success toast, but DO NOT use this for final verification
      },
      onFailure: (err: any) => {
        console.warn('CF onFailure (fallback):', err);
        // optional: show toast. Final state still comes from verify endpoint (redirect or webhook)
      },
      onClose: () => {
        console.log('CF onClose (fallback)');
      }
    };

      cf.checkout(options);
      this.loading = false;
    } catch (ex: any) {
      console.error('openCheckoutWithSdk error', ex);
      this.loading = false;
      this.error = 'Failed to initialize payment SDK: ' + (ex?.message ?? ex);
    }
  }

  retryOpenSdk() {
    this.error = null;
    // allow re-open without re-entering ngOnInit guard
    this.openCheckoutWithSdk();
  }

  goBack() {
    this.router.navigate(['/payment'], {
                  state: {
                plan: this.plan,
                status: this.status,                 // canonical status
              orderId: this.orderId,                  // use orderId variable returned by server
              userId: this.userId,               // user id (number)
              isMember: true,
     }});
  }

  ngOnDestroy(): void {
    this.subs.forEach((s) => s.unsubscribe());
    this.subs = [];
  }
}
