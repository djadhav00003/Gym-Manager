import { booleanAttribute, Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Member, Trainer } from '../../models/user.model';
import { HttpClient } from '@angular/common/http';
import { TrainerListComponent } from '../../trainer/trainer-list/trainer-list.component';
import { CommonModule } from '@angular/common';
import { AddTrainerComponent } from '../../trainer/add-trainer/add-trainer.component';
import { MemberListComponent } from '../../member/member-list/member-list.component';
import { AddPlanComponent } from '../../plans/add-plan.component';
import { AddImagesComponent } from '../../gym/add-images/add-images.component';
import { GalleriaModule } from 'primeng/galleria';
import { TrainerService } from '../../services/trainer.service';
import { GymService } from '../../services/gym.service';
import { MemberService } from '../../services/member.service';

@Component({
  selector: 'app-view-people',
  standalone: true,
  imports: [
    CommonModule,
    TrainerListComponent,
    AddTrainerComponent,
    MemberListComponent,
    AddPlanComponent,
    AddImagesComponent,
    GalleriaModule,
  ],
  templateUrl: './view-people.component.html',
  styleUrls: ['./view-people.component.css'],
})
export class ViewPeopleComponent implements OnInit {
  route = inject(ActivatedRoute);
  gymId: number = 0;
  trainers: Trainer[] = [];
  members: Member[] = [];
  showAddPanel: boolean = false;
  isMember: boolean = false;
  isAdmin: boolean = false;
  showAddPlanPanel: boolean = false;
  showAddImagePanel: boolean = false;
  images: any[] = [];
  email: string = '';
  userId: number=0;

  constructor(
    private http: HttpClient,
    private trainerService: TrainerService,
    private gymService: GymService,
    private memberService: MemberService,
    private router: Router,
  ) {}

  ngOnInit() {
      const state = history.state || {};
    if (state) {
       this.gymId   = Number(state['gymId']);
    this.userId  = Number(state['userId']);
    this.email   = String(state['email']);
    this.isMember= state['isMember'] === true || state['isMember'] === 'true';
    this.isAdmin= state['isAdmin'] === true || state['isAdmin'] === 'true';
    }
    this.fetchTrainers();
    this.fetchMembers();
    this.fetchGymImages();
  }

  fetchTrainers() {
    this.trainerService.getTrainersByGym(this.gymId).subscribe({
      next: (data) => {
        this.trainers = data;
      },
      error: (err) => {
        console.error('Error fetching trainers:', err);
      },
    });
  }
  fetchGymImages() {
    this.gymService.getGymImages(this.gymId).subscribe({
      next: (data) => {
        this.images = data.map((img: string) => ({
          itemImageSrc: img,
          thumbnailImageSrc: img,
        }));
      },
      error: (err) => {
        console.error('Error loading gym images:', err);
      },
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
    this.memberService.getMembersByGym(this.gymId).subscribe({
      next: (data) => {
        this.members = data;
      },
      error: (err) => {
        console.error('Error fetching members:', err);
        alert('Failed to load members.');
      },
    });
  }

  toggleImagePanel() {
    this.showAddImagePanel = !this.showAddImagePanel;
  }

  selectPlans() {
   this.router.navigate(['/plans'], {
  state: {
    gymId: this.gymId,
    userId: this.userId,
    email: this.email,
    isMember: this.isMember,
    isAdmin:this.isAdmin
  }
});
  }

  onImagesUploaded(files: string[]) {
    if (files && files.length > 0) {
      alert(`✅ Image uploaded successfully!`);
      const newImages = files.map((url) => ({
        itemImageSrc: url,
        thumbnailImageSrc: url,
      }));

      this.images = [...this.images, ...newImages];
    } else {
      alert('⚠️ No images were uploaded.');
    }
  }
}
