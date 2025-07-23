// profile.component.ts
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { User } from '../../models/user.model';
import { AuthService } from '../../services/auth.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent {
  @Input() user!: User;
  @Output() profileUpdated = new EventEmitter<User>();
  @Output() close = new EventEmitter<void>();

  editableUser!: User;

  constructor(private authService: AuthService) {}

  ngOnInit() {
    this.editableUser = { ...this.user }; // ✅ Initialize editable copy
  }

  cancelEdit() {
    this.editableUser = { ...this.user }; // ✅ Revert to original values
    this.closeProfile();
  }

  updateProfile() {
    const emailChanged = this.editableUser.email !== this.user.email;

    if (emailChanged) {
      this.authService.checkEmailExists(this.editableUser.email).subscribe(exists => {
        if (exists) {
          alert('Email is already taken.');
        } else {
          this.saveProfile();
        }
      });
    } else {
      this.saveProfile();
    }
  }

  saveProfile() {
    this.authService.updateUserProfile(this.editableUser).subscribe(updatedUser => {
      this.editableUser = { ...updatedUser }; // ✅ update local copy only
      this.profileUpdated.emit(updatedUser); // ✅ Notify parent
      alert('Profile updated successfully!');
      this.closeProfile();
    }, err => {
      console.error(err);
      alert('Something went wrong while updating.');
    });
  }

  closeProfile() {
    this.close.emit();
  }
}
