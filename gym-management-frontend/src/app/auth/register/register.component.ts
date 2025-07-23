import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router,RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';
import { User } from '../../models/user.model';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [RouterModule, FormsModule,CommonModule],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
})
export class RegisterComponent {
  user: User = {
    fullName: '',
    email: '',
    password: '',
    role: 'Member',
    gender: '',
    phoneNumber: '',
    dateOfBirth: '',
    address: ''
  };


  constructor(private authService: AuthService, private router: Router) {}

  register() {
    const { fullName, email, password, role, gender, phoneNumber, dateOfBirth, address } = this.user;

    if (!fullName || !email || !password || !role || !gender || !phoneNumber || !dateOfBirth || !address) {
      alert('Please fill all fields!');
      return;
    }

    this.authService.register(this.user).subscribe({
      next: (res) => {
        console.log('User registered:', res);
        alert('Registration successful!');
        this.router.navigate(['/login']);
      },
      error: (err) => {
        console.error('Registration failed:', err);
        alert(err.error || 'Registration failed.');
      }
    });
  }
}
