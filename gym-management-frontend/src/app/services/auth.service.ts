import { Injectable, Injector } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { tap, catchError, map, switchMap } from 'rxjs/operators';
import { User } from '../models/user.model';
import { shareReplay } from 'rxjs/operators';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
    private accessToken = '';
  private token$ = new BehaviorSubject<string | null>(null);
  tokenChanges$ = this.token$.asObservable();


  constructor(private injector: Injector) {}
// lazy getter for HttpClient to avoid constructor-level dependency
  private get http(): HttpClient {
    return this.injector.get(HttpClient);
  }
  private init$: Observable<any | null> | null = null;
  private loggedOut = false;
    // runtime getter for api base + convenience for user endpoints
  private get apiBase(): string {
    const cfg = (window as any).appConfig;
    // prefer config value, fallback to localhost dev
    const base = cfg && cfg.apiBaseUrl ? cfg.apiBaseUrl : 'https://localhost:7008';
    // ensure no trailing slash
    return base.replace(/\/$/, '');
  }

  private userEndpoint(path = ''): string {
    const prefix = `${this.apiBase}/api/User`;
    return path ? `${prefix}/${path}` : prefix;
  }


initialize(): Observable<any | null> {
  if (this.init$) return this.init$ as Observable<any | null>;
   // if user explicitly logged out, don't try silent refresh
  if (this.loggedOut) return of(false);

  this.init$ = this.refreshToken().pipe(
    switchMap((refreshRes: any) => {
      const token =
        refreshRes?.accessToken ??
        refreshRes?.AccessToken ??
        refreshRes?.access_token ??
        null;

      if (!token) return of(null);

      this.setAccessToken(token);

      // return the validate-token payload (user info)
      return this.checkSession().pipe(
        catchError(() => of(null))
      );
    }),
    catchError(() => of(null)),
    shareReplay({ bufferSize: 1, refCount: false })
  );

  return this.init$;
}
  register(user: User): Observable<any> {
    return this.http.post(this.userEndpoint('register'), user);
  }


  getUserByEmail(email: string) {
    return this.http.get<User>(this.userEndpoint(`getUserByEmail/${encodeURIComponent(email)}`));
  }

  checkEmailExists(email: string) {
    return this.http.get<boolean>(`${this.userEndpoint('check-email')}?email=${encodeURIComponent(email)}`);
  }

  updateUserProfile(user: User) {
    return this.http.put<User>(this.userEndpoint('update-profile'), user);
  }
  login(payload: { email: string; password: string; role: string }): Observable<any> {
    // Backend returns accessToken in body and sets RefreshToken cookie
    return this.http.post<any>(this.userEndpoint('login'), payload, { withCredentials: true }).pipe(
      tap(res => {
        const token = res?.accessToken ?? res?.AccessToken ?? null;
        if (token) this.setAccessToken(token);
        this.loggedOut = false; // allow silent refreshs again in future
        this.init$ = null;
      })
    );
  }
   refreshToken() {
    return this.http.post<any>(this.userEndpoint('refresh-token'), {}, { withCredentials: true });
  }

    checkSession() {
    return this.http.get<any>(this.userEndpoint('validate-token'), { withCredentials: true });
  }

  // uploadGymImages(gymId: number, imageUrls: string[]): Observable<any> {
  //   const body = { imageUrls };
  //   return this.http.post(`${this.apiUrl}/${gymId}/images`, body);
  // }

   logout(): Observable<any> {
    return this.http.post<any>(this.userEndpoint('logout'), {}, { withCredentials: true }).pipe(
      tap(() => {
        // clear client-side auth state immediately
        this.clearAuth();

        // prevent silent refresh after logout
        this.loggedOut = true;

        // reset init$ so future initialize() calls return quickly
        this.init$ = of(false);

        // best-effort client-side cookie deletion (must match path/domain used by server)
        try {
          document.cookie = 'RefreshToken=; Path=/; Expires=Thu, 01 Jan 1970 00:00:00 GMT; SameSite=None; Secure';
        } catch (e) { /* ignore */ }
      })
    );
  }


   getAccessToken(): string | null {
    return this.accessToken || null;
  }

  setAccessToken(token: string) {
    this.accessToken = token;
    this.token$.next(token);
  }

  clearAuth() {
    this.accessToken = '';
    this.token$.next(null);
  }

  isLoggedIn(): boolean {
    return !!this.accessToken;
  }

}
