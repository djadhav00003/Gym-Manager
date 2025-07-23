import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Trainer } from '../../models/user.model';


@Component({
  standalone: true,
  selector: 'app-trainer-list',
  imports: [CommonModule],
  templateUrl: './trainer-list.component.html',
  styleUrls: ['./trainer-list.component.css']
})
export class TrainerListComponent {
  @Input() trainers: Trainer[] = [];
}
