import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';


@Injectable({
  providedIn: 'root',
})
export class GymService {

  constructor(private http: HttpClient) {}
 // runtime base getter (reads window.appConfig set by loader)
  private get apiBase(): string {
    const cfg = (window as any).appConfig || {};
    const base = cfg.apiBaseUrl || 'https://localhost:7008';
    return base.replace(/\/$/, '');
  }

  // helper for gym controller
  private gymEndpoint(path = ''): string {
    const prefix = `${this.apiBase}/api/Gym`;
    return path ? `${prefix}/${path}` : prefix;
  }


  // Upload images (no cookies needed unless your server requires them)
  uploadGymImages(gymId: number, imageUrls: string[]): Observable<any> {
    const body = { imageUrls };
    return this.http.post(`${this.gymEndpoint(`${gymId}/images`)}`, body);
  }

  addGym(gym: any, userId: number): Observable<any> {
    // If your backend requires auth via HttpOnly cookie, add { withCredentials: true } here.
    return this.http.post<any>(`${this.gymEndpoint(`add/${userId}`)}`, gym);
  }

  updateGym(gym: any): Observable<any> {
    return this.http.put<any>(this.gymEndpoint('update'), gym);
  }

  deleteGym(gymId: number, userId: number): Observable<string> {
    return this.http.delete(`${this.gymEndpoint(`delete/${gymId}/${userId}`)}`, {
      responseType: 'text'
    });
  }

  // Get all gyms
  getAllGyms(): Observable<any[]> {
    return this.http.get<any[]>(this.gymEndpoint('all'));
  }

  getGymsByOwner(userId: number): Observable<any[]> {
    return this.http.get<any[]>(this.gymEndpoint(`owner/${userId}`), {
    withCredentials: true
  });
  }

  GetGymsExceptUser(userId: number): Observable<any[]> {
    return this.http.get<any[]>(this.gymEndpoint(`all-except-user/${userId}`));
  }

  // Get all gyms with membership info for a specific user
  getGymsWithMembership(userId: number): Observable<any[]> {
    return this.http.get<any[]>(this.gymEndpoint(`withMembership/${userId}`));
  }

  getGymImages(gymId: number): Observable<string[]> {
    return this.http.get<string[]>(this.gymEndpoint(`images/${gymId}`));
  }
}
