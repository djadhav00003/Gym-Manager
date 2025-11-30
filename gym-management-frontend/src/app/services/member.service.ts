import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Member } from '../models/user.model';


@Injectable({
  providedIn: 'root',
})
export class MemberService  {


  constructor(private http: HttpClient) {}

  // runtime base getter (from app-config.json → window.appConfig)
  private get apiBase(): string {
    const cfg = (window as any).appConfig || {};
    const base = cfg.apiBaseUrl || 'https://localhost:7008';
    return base.replace(/\/$/, '');
  }

  // helper: build /api/Member endpoints
  private memberEndpoint(path = ''): string {
    const prefix = `${this.apiBase}/api/Member`;
    return path ? `${prefix}/${path}` : prefix;
  }

  addMember(memberRequest: any): Observable<string> {
    return this.http.post(this.memberEndpoint(), memberRequest, {
      responseType: 'text',
    });
  }

  // --------------------------------------------
  // 🔹 CHECK IF USER IS MEMBER OF GYM
  // --------------------------------------------
  isUserMember(userId: number, gymId: number): Observable<boolean> {
    const url = this.memberEndpoint(`isMember?userId=${userId}&gymId=${gymId}`);
    return this.http.get<boolean>(url);
  }

  // --------------------------------------------
  // 🔹 GET ALL MEMBERS OF A GYM
  // --------------------------------------------
  getMembersByGym(gymId: number): Observable<Member[]> {
    return this.http.get<Member[]>(this.memberEndpoint(`gym/${gymId}`));
  }


}
