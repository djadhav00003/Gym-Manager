import { Component, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';

import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { catchError, map, of, switchMap, throwError } from 'rxjs';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [RouterModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
})
export class LoginComponent implements OnInit {
  email: string = '';
  password: string = '';
  role: string = '';

  constructor(private authService: AuthService, private router: Router) {}

  ngOnInit() {
    this.authService.initialize().subscribe((userPayload) => {
       console.log('userpayload',userPayload);
      if (userPayload) {
        // userPayload contains email/role/id from validate-token — navigate directly
        this.navigateBasedOnRole(userPayload);
      } else {
        // not authenticated, stay on login
      }
    });
  }

  login() {
    if (!this.email || !this.password || !this.role) {
      alert('Please fill in all fields including role.');
      return;
    }

    const user = {
      fullName: '', // optional field
      email: this.email,
      password: this.password,
      role: this.role,
    };

    const toBool = (v: any) => v === true || v === 'true';

    this.authService.login(user).subscribe({
      next: (res) => {
        console.log('Login successful:', res);
        alert('Login successful!');

        if (res.role === 'Admin') {
          this.router.navigate(['/home/admin-dashboard'], {
            state: {
              email: res.email,
              userId: res.id,
              isAdmin: toBool(res.isAdmin),
              isMember: toBool(res.isMember),
            },
          });
        } else if (res.role === 'Gymowner') {
          console.log(res.role);
          this.router.navigate(['/home/gym-owner-dashboard'], {
            state: {
    email: res.email,
    userId: res.id,
    isMember: toBool(res.isMember),
    isAdmin: toBool(res.isAdmin),
  },
          });
        } else {
          console.log('1 member',res.role);
          this.router.navigate(['/home/member-dashboard'], {
             state: {
    email: res.email,
    userId: res.id,
    isMember: toBool(res.isMember),
    isAdmin: toBool(res.isAdmin)
  }
          });
        }
      },
      error: (err) => {
        console.error('Login failed:', err);
        alert(err.error || 'Login failed. Please check credentials.');
      },
    });
  }

  private navigateBasedOnRole(res: any) {
    const toBool = (v: any) => v === true || v === 'true';
    if (res.role === 'Admin') {
      this.router.navigate(['/home/admin-dashboard'], {
        state: {
          email: res.email,
          userId: res.id,
          isAdmin: toBool(res.isAdmin),
          isMember: toBool(res.isMember),
        },
      });
    } else if (res.role === 'Gymowner') {
    console.log('gymowner2');
    console.log(res);
      this.router.navigate(['/home/gym-owner-dashboard'], {
        state: {
    email: res.email,
    userId: res.id,
    isMember: toBool(res.isMember),
    isAdmin: toBool(res.isAdmin),
  },
      });
    } else {
          console.log('2 member',res.role);
      this.router.navigate(['/home/member-dashboard'], {
          state: {
    email: res.email,
    userId: res.id,
    isMember: toBool(res.isMember),
    isAdmin: toBool(res.isAdmin)
  }
      });
    }
  }
}
