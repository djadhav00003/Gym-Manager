import { booleanAttribute, Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Member, Trainer } from '../../models/user.model';
import { HttpClient } from '@angular/common/http';
import { TrainerListComponent } from '../../trainer/trainer-list/trainer-list.component';
import { CommonModule } from '@angular/common';
import { AddTrainerComponent } from '../../trainer/add-trainer/add-trainer.component';
import { MemberListComponent } from '../../member/member-list/member-list.component';
import { AddPlanComponent } from '../../plans/add-plan.component';


@Component({
  selector: 'app-view-people',
  standalone: true,
   imports: [CommonModule,TrainerListComponent,AddTrainerComponent,MemberListComponent,AddPlanComponent
      ],
  templateUrl: './view-people.component.html',
  styleUrls: ['./view-people.component.css']
})
export class ViewPeopleComponent implements OnInit {
  route = inject(ActivatedRoute);
  gymId: number=0;
  trainers: Trainer[] = [];
  members: Member[] = [];
  showAddPanel: boolean = false;
  isMember: boolean = false;
  showAddPlanPanel: boolean = false;

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      this.gymId = Number(params.get('gymId'));
      this.isMember = booleanAttribute(params.get('isMember'));
      console.log('Selected Gym ID:', this.gymId);
      this.fetchTrainers();
      this.fetchMembers();
    });

  }

  fetchTrainers() {
    this.http.get<Trainer[]>(`https://localhost:7008/api/Trainer/all/${this.gymId}`)
      .subscribe((data) => {
        this.trainers = data;
      }, error => {
        console.error('Error fetching trainers:', error);
      });
  }

  toggleAddPanel() {
    this.showAddPanel = !this.showAddPanel;
  }
  togglePlanPanel() {
    this.showAddPlanPanel = !this.showAddPlanPanel;
  }

  onTrainerAdded() {
    this.fetchTrainers();
    this.toggleAddPanel(); // close panel after adding
  }

  fetchMembers() {
    this.http.get<Member[]>(`https://localhost:7008/api/Member/gym/${this.gymId}`)
      .subscribe(data => {
        this.members = data;
      }, error => {
        console.error('Error fetching members:', error);
      });
  }



}
