import { Injectable, Injector } from '@angular/core';
import {
  HttpEvent, HttpHandler, HttpInterceptor, HttpRequest, HttpErrorResponse
} from '@angular/common/http';
import { Observable, BehaviorSubject, throwError } from 'rxjs';
import { catchError, switchMap, filter, take, finalize } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';

const getApiBase = () => {
  const cfg = (window as any).appConfig || {};
  return (cfg.apiBaseUrl || 'https://localhost:7008').replace(/\/$/, '');
};
@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  private refreshInProgress = false;
  private refreshSubject = new BehaviorSubject<string | null>(null);

  private readonly API_BASE = getApiBase();
  // inject Injector, NOT AuthService directly to avoid circular DI
  constructor(private injector: Injector) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const auth = this.injector.get(AuthService);

  // mark as API request if URL matches runtime base OR if it's a relative /api/ call
    let isApiRequest = false;
    try {
      const absoluteUrl = new URL(req.url, window.location.origin).toString();
      isApiRequest = absoluteUrl.startsWith(this.API_BASE) || req.url.startsWith('/api/');
    } catch {
      isApiRequest = req.url.startsWith(this.API_BASE) || req.url.startsWith('/api/');
    }
    if (!isApiRequest) return next.handle(req);

      const isAuthEndpoint = /\/login|\/refresh-token/.test(req.url);

    let authReq = req;
    const token = auth.getAccessToken();

    if (!isAuthEndpoint && token) {
      authReq = req.clone({
        setHeaders: { Authorization: `Bearer ${token}` }
      });
    }

    return next.handle(authReq).pipe(
      catchError(err => {
        if (err instanceof HttpErrorResponse && err.status === 401) {
          // for auth endpoints we should not attempt refresh
          if (isAuthEndpoint) {
            return throwError(() => err);
          }
          return this.handle401Error(authReq, next);
        }
        return throwError(() => err);
      })
    );
  }

  private handle401Error(req: HttpRequest<any>, next: HttpHandler): Observable<any> {
    const auth = this.injector.get(AuthService);

    if (!this.refreshInProgress) {
      this.refreshInProgress = true;
      this.refreshSubject.next(null);

      return auth.refreshToken().pipe(
        switchMap((res: any) => {
          // tolerate different casing / shapes
          const newToken = res?.accessToken ?? res?.AccessToken ?? res?.access_token ?? null;
          if (!newToken) {
            auth.clearAuth();
            return throwError(() => new Error('Refresh failed'));
          }

          auth.setAccessToken(newToken);
          this.refreshSubject.next(newToken);
           return next.handle(req.clone({ setHeaders: { Authorization: `Bearer ${newToken}` } }));
          }),
        catchError(refreshErr => {
          auth.clearAuth();
          return throwError(() => refreshErr);
        }),
        finalize(() => { this.refreshInProgress = false; })
      );

    } else {
      // wait for the ongoing refresh and then retry
      return this.refreshSubject.pipe(
        filter(token => token !== null),
        take(1),
        switchMap((token) => next.handle(req.clone({
          setHeaders: { Authorization: `Bearer ${token as string}` }
        })))
      );
    }
  }
}
