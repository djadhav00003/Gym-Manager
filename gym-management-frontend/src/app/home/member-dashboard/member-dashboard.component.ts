import { Component } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { User } from '../../models/user.model';
import { ProfileComponent } from '../../profile/profile/profile.component';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { GymListComponent } from '../../gym/gym-list/gym-list.component';
import { GymService } from '../../services/gym.service';

@Component({
  selector: 'app-member-dashboard',
  standalone: true,
  imports: [CommonModule, ProfileComponent, GymListComponent],
  templateUrl: './member-dashboard.component.html',
  styleUrls: ['./member-dashboard.component.scss']
})
export class MemberDashboardComponent {
  profileOpen: boolean = false;
  memberUser!: User;
  gyms: any[] = [];
  gymsWithMembership: any[] = [];
  isAdmin:boolean=false;
  userId!:number;
  isMember:boolean=false;
  email:string='';

  constructor(private route: ActivatedRoute, private authService: AuthService, private http: HttpClient, private router: Router,
     private gymService:GymService
  ) {}

  ngOnInit(): void {
     const state: any = history.state;

  this.email = state.email;
  this.userId = state.userId;
  this.isMember = state.isMember;
  this.isAdmin = state.isAdmin;
      if (this.email) {
        this.authService.getUserByEmail(this.email).subscribe(user => {
          this.memberUser = user;
        });
      }

  if(this.isMember){
    this.fetchGymWithMembership(this.userId);
  }
    this.GetGymsExceptUser();
  }

  toggleProfile() {
    this.profileOpen = !this.profileOpen;
  }

  onProfileUpdated(updatedUser: User) {
    this.memberUser = { ...updatedUser }; // Update local copy used in dashboard
  }
  GetGymsExceptUser() {
     this.gymService.GetGymsExceptUser(this.userId).subscribe({
    next: (data) => {
      this.gyms = data;
    },
    error: (err) => {
      console.error('❌ Error fetching gyms:', err);
      alert('Failed to load gyms.');
    }
  });
  }

  fetchGymWithMembership(userId: number) {
    this.gymService.getGymsWithMembership(userId).subscribe({
    next: (data) => {
      this.gymsWithMembership = data;
    },
    error: (err) => {
      console.error('❌ Error fetching gyms with membership:', err);
      alert('Failed to load gym memberships.');
    }
  });
  }

  logOut() {
     this.authService.logout().subscribe({
      next: () => {
        console.log('Logged out');
        this.router.navigate(['/login']);
      },
      error: (err) => {
        console.error('Logout failed', err);
        this.router.navigate(['/login']);
      }
    });
  }
}
