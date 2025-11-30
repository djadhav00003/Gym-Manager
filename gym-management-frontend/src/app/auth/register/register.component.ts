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

  onDateInput(event: any) {
  let value = event.target.value;

  // If user is typing numbers like 21102001
  if (/^\d{8}$/.test(value)) {
    const day = value.substring(0, 2);
    const month = value.substring(2, 4);
    const year = value.substring(4, 8);
    value = `${year}-${month}-${day}`;
  }

  // If user types date with slash or dash
  if (/^\d{1,2}[-/]\d{1,2}[-/]\d{4}$/.test(value)) {
    const parts = value.split(/[-/]/);
    const day = parts[0].padStart(2, '0');
    const month = parts[1].padStart(2, '0');
    const year = parts[2];
    value = `${year}-${month}-${day}`;
  }

  // Final validation (YYYY-MM-DD)
  if (/^\d{4}-\d{2}-\d{2}$/.test(value)) {
    this.user.dateOfBirth = value;
  }
}

}
