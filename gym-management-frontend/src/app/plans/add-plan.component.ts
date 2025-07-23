import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

import { HttpClient, HttpClientModule } from '@angular/common/http';
import { Trainer } from '../models/user.model';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-add-plan',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, HttpClientModule],
  templateUrl: './add-plan.component.html',
  styleUrls: ['./add-plan.component.css'],
})
export class AddPlanComponent {
  @Input() gymId!: number;
  @Input() trainers: Trainer[] = [];
  @Output() close = new EventEmitter();
  @Output() planAdded = new EventEmitter();

  planForm: FormGroup;

  constructor(private fb: FormBuilder, private http: HttpClient) {
    this.planForm = this.fb.group({
      PlanName: ['', Validators.required],
      DurationInDays: [null, Validators.required],
      Price: [null, Validators.required],
      IsPersonalTrainerAvailable: [false],
      TrainerId: [{ value: null, disabled: true }],

    });
  }

  onCheckboxChange(event: any) {
    const checked = event.target.checked;
    if (checked) {
      this.planForm.get('TrainerId')?.enable();
    } else {
      this.planForm.get('TrainerId')?.disable();
      this.planForm.get('TrainerId')?.reset();
    }
  }

  submit() {
    if (this.planForm.valid) {
      const formValue = { ...this.planForm.value, gymId: this.gymId };
      this.http.post('https://localhost:7008/api/Payment/add', formValue).subscribe({
        next: () => {
          alert('Plan added successfully!');
          this.planAdded.emit();
          this.planForm.reset();
          this.planForm.get('TrainerId')?.disable(); // Ensure correct key casing if used in form
        },
        error: (err) => {
          console.error('Failed to add plan:', err);
          alert('Failed to add plan. Please try again.');
        }
      });
    }
  }

  closePanel() {
    this.close.emit();
  }
}
