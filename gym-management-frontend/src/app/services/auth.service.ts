import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User } from '../models/user.model';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private apiUrl = 'https://localhost:7008/api/User'; // Replace with your actual API URL

  constructor(private http: HttpClient) {}

  register(user: User): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, user);
  }


  login(user: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/login`, user);
  }

  getUserByEmail(email: string) {
    return this.http.get<User>(`${this.apiUrl}/getUserByEmail/${email}`);
  }

  checkEmailExists(email: string) {
    return this.http.get<boolean>(`${this.apiUrl}/check-email?email=${encodeURIComponent(email)}`);
  }

  updateUserProfile(user: User) {
    return this.http.put<User>(`${this.apiUrl}/update-profile`, user);
  }
}
