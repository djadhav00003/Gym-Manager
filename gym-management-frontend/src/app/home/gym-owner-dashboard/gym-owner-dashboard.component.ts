import { Component } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { User } from '../../models/user.model';
import { ProfileComponent } from '../../profile/profile/profile.component';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { GymListComponent } from '../../gym/gym-list/gym-list.component';
import { AddGymComponent } from '../../gym/add-gym/add-gym.component';
import { GymService } from '../../services/gym.service';

@Component({
  selector: 'app-gym-owner-dashboard',
  standalone: true,
  imports: [CommonModule, ProfileComponent,GymListComponent,
    AddGymComponent],
  templateUrl: './gym-owner-dashboard.component.html',
  styleUrl: './gym-owner-dashboard.component.css'
})
export class GymOwnerDashboardComponent {

   profileOpen: boolean = false;
    adminUser!: User;
    gyms: any[] = [];
    showAddGymPanel: boolean = false;
    isAdmin:boolean=false;
    userId!:number;
    isMember:boolean=false;
   email:string="";

  constructor(private route: ActivatedRoute,private authService: AuthService,private http: HttpClient, private router: Router,
    private gymService:GymService
  ) {}

     ngOnInit(): void {
     const state: any = history.state;

  this.email = state.email;
  this.userId = state.userId;
  this.isMember = state.isMember;
  this.isAdmin=state.isAdmin;
      if (this.email) {
        this.authService.getUserByEmail(this.email).subscribe(user => {
          this.adminUser = user;
        });
      }

    this.fetchGyms();
  }

  toggleProfile() {
    this.profileOpen = !this.profileOpen;
  }



  fetchGyms() {
    this.gymService.getGymsByOwner(this.userId).subscribe({
    next: (data) => {
      this.gyms = data;
    },
    error: (err) => {
      console.error('❌ Error fetching gyms:', err);
      alert('Failed to load gyms.');
    }
  });
  }

  toggleAddGymPanel() {
    this.showAddGymPanel = !this.showAddGymPanel;
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

  onGymAdded() {
    this.fetchGyms();
    this.toggleAddGymPanel();
  }


}
