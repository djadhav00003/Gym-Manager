import { Routes } from '@angular/router';
import { AdminDashboardComponent } from './home/admin-dashboard/admin-dashboard.component';
import { MemberDashboardComponent } from './home/member-dashboard/member-dashboard.component';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () =>
      import('./auth/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./auth/register/register.component').then((m) => m.RegisterComponent),
  },
  {
    path: 'gyms',
    loadComponent: () => import('./gym/gym-list/gym-list.component').then(m => m.GymListComponent)
  },
  {
    path: 'add-gym',
    loadComponent: () => import('./gym/add-gym/add-gym.component').then(m => m.AddGymComponent)
  },
  {
    path: 'trainers',
    loadComponent: () => import('./trainer/trainer-list/trainer-list.component').then(m => m.TrainerListComponent)
  },
  {
    path: 'add-trainer',
    loadComponent: () => import('./trainer/add-trainer/add-trainer.component').then(m => m.AddTrainerComponent)
  },
  {
    path: 'members',
    loadComponent: () => import('./member/member-list/member-list.component').then(m => m.MemberListComponent)
  },
  {
    path: 'add-member',
    loadComponent: () => import('./member/add-member/add-member.component').then(m => m.AddMemberComponent)
  },
  {
    path: 'plans/:gymId/:userId/:email/:isMember',
    loadComponent: () => import('./plans/plan-selection/plan-selection.component').then(m => m.PlanSelectionComponent)
  },
  {
    path: 'payment',
    loadComponent: () => import('./payment/payment/payment.component').then(m => m.PaymentComponent)
  },
  {
    path: 'profile',
    loadComponent: () => import('./profile/profile/profile.component').then(m => m.ProfileComponent)
  },
  {
    path: 'home/admin-dashboard',
    component: AdminDashboardComponent
  },
  {
    path: 'home/member-dashboard',
    component: MemberDashboardComponent
  },
  {
    path: 'view-people/:gymId/:userId/:isMember',
    loadComponent: () =>
      import('./shared/view-people/view-people.component').then(m => m.ViewPeopleComponent)
  }
];
