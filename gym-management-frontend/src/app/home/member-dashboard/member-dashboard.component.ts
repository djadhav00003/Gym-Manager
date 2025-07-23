import { Component } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { User } from '../../models/user.model';
import { ProfileComponent } from '../../profile/profile/profile.component';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { GymListComponent } from '../../gym/gym-list/gym-list.component';

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

  constructor(private route: ActivatedRoute, private authService: AuthService, private http: HttpClient, private router: Router) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.email = params['email'];
      this.userId=params['userId'];
      this.isMember=params['isMember'];
      if (this.email) {
        this.authService.getUserByEmail(this.email).subscribe(user => {
          this.memberUser = user;
        });
      }
    });
  if(this.isMember){
    this.fetchGymWithMembership(this.userId);
  }
    this.fetchGyms();
  }

  toggleProfile() {
    this.profileOpen = !this.profileOpen;
  }

  onProfileUpdated(updatedUser: User) {
    this.memberUser = { ...updatedUser }; // Update local copy used in dashboard
  }
  fetchGyms() {
    this.http.get<any[]>('https://localhost:7008/api/Gym/all')
      .subscribe((data) => {
        this.gyms = data;
      });
  }

  fetchGymWithMembership(userId: number) {
    this.http.get<any[]>(`https://localhost:7008/api/Gym/withMembership/${userId}`)
      .subscribe((data) => {
        this.gymsWithMembership = data;
      });
  }

  logOut() {
    this.router.navigate(['/login']);
  }
}
