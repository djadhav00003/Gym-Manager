import { Component, EventEmitter, Input, Output } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-add-trainer',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './add-trainer.component.html',
  styleUrls: ['./add-trainer.component.css'],

})
export class AddTrainerComponent {
  @Input() gymId!: number; //
  @Output() close = new EventEmitter<void>();
  @Output() trainerAdded = new EventEmitter<void>();

  trainerForm: FormGroup;

  constructor(private fb: FormBuilder, private http: HttpClient) {
    this.trainerForm = this.fb.group({
      TrainerName: ['', Validators.required],
      Speciality: ['', Validators.required],
      PhoneNumber: ['', Validators.required],
      Experience: ['', [Validators.required, Validators.min(0)]],
      GymId: 0
    });
  }

  submit() {
    if (this.trainerForm.valid) {
      const trainerData = this.trainerForm.value;

      // Append GymId manually just to be safe
      trainerData.gymId = this.gymId;
this.http.post(`https://localhost:7008/api/Trainer/add/${this.gymId}`, trainerData)
  .subscribe({
    next: () => {
      alert('Trainer added successfully!');
      this.trainerAdded.emit();
      this.trainerForm.reset();
    },
    error: (err) => {
      console.error('Failed to add trainer:', err);
      alert('Failed to add trainer. Please try again.');
    }
  });
    }
  }

  closePanel() {
    this.close.emit();
  }
}
