import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

// Import your components here
import { LoginComponent } from './auth/login/login.component';
import { RegisterComponent } from './auth/register/register.component';
import { GymListComponent } from './gym/gym-list/gym-list.component';
import { AddGymComponent } from './gym/add-gym/add-gym.component';
import { TrainerListComponent } from './trainer/trainer-list/trainer-list.component';
import { AddTrainerComponent } from './trainer/add-trainer/add-trainer.component';
import { MemberListComponent } from './member/member-list/member-list.component';
import { AddMemberComponent } from './member/add-member/add-member.component';
import { PlanSelectionComponent } from './plans/plan-selection/plan-selection.component';
import { PaymentComponent } from './payment/payment/payment.component';
import { ProfileComponent } from './profile/profile/profile.component';

const routes: Routes = [
  { path: '', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'gyms', component: GymListComponent },
  { path: 'add-gym', component: AddGymComponent },
  { path: 'trainers', component: TrainerListComponent },
  { path: 'add-trainer', component: AddTrainerComponent },
  { path: 'members', component: MemberListComponent },
  { path: 'add-member', component: AddMemberComponent },
  { path: 'plans', component: PlanSelectionComponent },
  { path: 'payment', component: PaymentComponent },
  { path: 'profile', component: ProfileComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
