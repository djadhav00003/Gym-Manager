import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Plan } from '../models/user.model';


@Injectable({
  providedIn: 'root',
})
export class PlanService  {


  constructor(private http: HttpClient) {}

   // runtime API base (from app-config.json)
  private get apiBase(): string {
    const cfg = (window as any).appConfig || {};
    const base = cfg.apiBaseUrl || 'https://localhost:7008';
    return base.replace(/\/$/, '');
  }

  // helper to build endpoint URLs
  private planEndpoint(path = ''): string {
    const prefix = `${this.apiBase}/api/Plan`;
    return path ? `${prefix}/${path}` : prefix;
  }

  // Add a new plan
  addPlan(payload: any): Observable<any> {
    // no credentials needed
    return this.http.post<any>(this.planEndpoint('add'), payload);
  }

  // Get plans for a specific gym
  getPlansByGym(gymId: number): Observable<Plan[]> {
    return this.http.get<Plan[]>(this.planEndpoint(`gym/${encodeURIComponent(gymId)}`));
  }

}
