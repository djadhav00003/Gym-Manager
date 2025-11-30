import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Plan } from '../models/user.model';

export interface CreateOrderResponse {
  // Cashfree session token (preferred)
  payment_session_id?: string;
  // alternative keys (some older code used sessionId or payment_session_id)
  sessionId?: string;

  // order id from Cashfree
  order_id?: string;
  orderId?: string;

  // optional hosted checkout
  checkoutLink?: string;

  // your local DB id
  paymentId?: number;

  amount?: number;
  currency?: string;

  // debug
  raw?: string;
}

export interface VerifyOrderResponse {
  status: 'SUCCESS' | 'FAILED' | 'PENDING' | string;
  orderId?: string;
  transactions?: any;
  raw?: any;
}

@Injectable({
  providedIn: 'root',
})
export class PaymentService  {

  constructor(private http: HttpClient) {}

    // runtime API base (from app-config.json)
  private get apiBase(): string {
    const cfg = (window as any).appConfig || {};
    const base = cfg.apiBaseUrl || 'https://localhost:7008';
    return base.replace(/\/$/, '');
  }

  // route builder
  private endpoint(path: string): string {
    return `${this.apiBase}/api/Payments/${path}`;
  }


   createOrder(payload: { userId: number; planId?: number; amount: number }):
      Observable<CreateOrderResponse> {
    return this.http.post<CreateOrderResponse>(
      this.endpoint('create-order'),
      payload,
      { withCredentials: true }
    );
  }

  /** Verify order: user must be logged in (cookie) */
  verifyOrder(orderId: string): Observable<VerifyOrderResponse> {
    return this.http.get<VerifyOrderResponse>(
      this.endpoint(`verify-order/${encodeURIComponent(orderId)}`),
      { withCredentials: true }
    );
  }
}
