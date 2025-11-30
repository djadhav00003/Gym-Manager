import { Component } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { User } from '../../models/user.model';
import { ProfileComponent } from '../../profile/profile/profile.component';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { GymListComponent } from '../../gym/gym-list/gym-list.component';
import { AddGymComponent } from '../../gym/add-gym/add-gym.component';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, ProfileComponent,GymListComponent,
    AddGymComponent],
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.scss']

})
export class AdminDashboardComponent {
  profileOpen: boolean = false;
  adminUser!: User;
  gyms: any[] = [];
  showAddGymPanel: boolean = false;
  isAdmin:boolean=false;
  userId!:number;
  isMember:boolean=false;
  email:string="";

  constructor(private route: ActivatedRoute,private authService: AuthService,private http: HttpClient, private router: Router) {}

  ngOnInit(): void {
   const state: any = this.router.getCurrentNavigation()?.extras?.state || history.state;
  this.email = state.email;
  this.userId = state.userId;
  this.isAdmin = state.isAdmin;
  this.isMember=state.isMember
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
    this.http.get<any[]>('https://localhost:7008/api/Gym/all')
      .subscribe((data) => {
        this.gyms = data;
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
