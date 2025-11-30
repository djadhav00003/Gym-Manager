// trainer.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Trainer } from '../models/user.model';


@Injectable({
  providedIn: 'root'
})
export class TrainerService {


  constructor(private http: HttpClient) {}

    // runtime API base from app-config.json
  private get apiBase(): string {
    const cfg = (window as any).appConfig || {};
    const base = cfg.apiBaseUrl || 'https://localhost:7008'; // fallback local
    return base.replace(/\/$/, '');
  }

  // helper to build Trainer API routes
  private trainerEndpoint(path = ''): string {
    const prefix = `${this.apiBase}/api/Trainer`;
    return path ? `${prefix}/${path}` : prefix;
  }

  // Get all trainers for a gym
  getTrainersByGym(gymId: number): Observable<Trainer[]> {
    return this.http.get<Trainer[]>(this.trainerEndpoint(`all/${encodeURIComponent(gymId)}`));
  }

  // Add new trainer
  addTrainer(gymId: number, trainerData: Trainer): Observable<any> {
    return this.http.post<any>(this.trainerEndpoint(`add/${encodeURIComponent(gymId)}`), trainerData);
  }

}
