import { Component, EventEmitter, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpClientModule } from '@angular/common/http';

@Component({
  selector: 'app-add-gym',
  standalone: true,
  imports: [CommonModule, FormsModule, HttpClientModule],
  templateUrl: './add-gym.component.html',
  styleUrls: ['./add-gym.component.css']
})
export class AddGymComponent {
  @Output() close = new EventEmitter<void>();
  @Output() gymAdded = new EventEmitter<void>();

  gym = {
    gymName: '',
    location: '',
    contact: '',
    facilities: ''
  };

  constructor(private http: HttpClient) {}

  addGym() {
    if (!this.gym.gymName || !this.gym.location || !this.gym.contact || !this.gym.facilities) {
      alert('Please fill all fields!');
      return;
    }

    this.http.post('https://localhost:7008/api/Gym/add', this.gym).subscribe({
      next: (res) => {
        alert('Gym added successfully!');
        this.gym = {
          gymName: '',
          location: '',
          contact: '',
          facilities: ''
        };
        this.gymAdded.emit();

      },
      error: (err) => {
        console.error('Error adding gym:', err);
        alert('Failed to add gym.');
      }
    });
  }

  cancel() {
    this.close.emit();
  }
}
