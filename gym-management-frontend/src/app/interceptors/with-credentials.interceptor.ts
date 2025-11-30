import { Injectable } from '@angular/core';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';

const getApiBase = () => {
  const cfg = (window as any).appConfig || {};
  return (cfg.apiBaseUrl || 'https://localhost:7008').replace(/\/$/, '');
};


@Injectable()
export class WithCredentialsInterceptor implements HttpInterceptor {
  private readonly API_BASE = getApiBase();
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
   try {
      // normalize absolute/relative urls
      const absoluteUrl = new URL(req.url, window.location.origin).toString();
      if (absoluteUrl.startsWith(this.API_BASE) || req.url.startsWith('/api/')) {
        const cloned = req.clone({ withCredentials: true });
        return next.handle(cloned);
      }
    } catch (e) {
      // if URL parsing fails, fall back to original check
      if (req.url.startsWith(this.API_BASE)) {
        const cloned = req.clone({ withCredentials: true });
        return next.handle(cloned);
      }
    }
    return next.handle(req);
  }
  }
