import { Component } from '@angular/core';
import { Router,RouterModule } from '@angular/router';

import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [RouterModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  email: string = '';
  password: string = '';
  role: string = '';

  constructor(private authService: AuthService, private router: Router) {}

  login() {
    if (!this.email || !this.password || !this.role) {
      alert('Please fill in all fields including role.');
      return;
    }

    const user = {
      fullName: '',        // Add placeholder for optional fields
      email: this.email,
      password: this.password,
      role: this.role
    };

    this.authService.login(user).subscribe({
      next: (res) => {
        console.log('Login successful:', res);
        alert('Login successful!');
        if (res.role === 'Admin') {
          this.router.navigate(['/home/admin-dashboard'], { queryParams: { email: res.email,userId: res.id ,isMember:res.isMember} });
        } else {
          this.router.navigate(['/home/member-dashboard'], { queryParams: { email: res.email,userId: res.id ,isMember:res.isMember } });
        }
      },
      error: (err) => {
        console.error('Login failed:', err);
        alert(err.error || 'Login failed. Please check credentials.');
      }
    });
  }
}
