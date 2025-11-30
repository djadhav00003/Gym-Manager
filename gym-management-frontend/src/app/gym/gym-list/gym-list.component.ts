import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { GymService } from '../../services/gym.service';

@Component({
  selector: 'app-gym-list',
  standalone: true,
  imports: [CommonModule,RouterModule,FormsModule],
  templateUrl: './gym-list.component.html',
  styleUrls: ['./gym-list.component.css']
})
export class GymListComponent {
  @Input() gyms: any[] = [];
  @Input() gymsWithMembership: any[] = [];
  @Input() isAdmin: boolean = false;
  @Input() isMember: boolean=false;
  @Input() userId!: number ;
  @Input() email: string='';

  editIndexMap: { [index: number]: boolean } = {};

  constructor(private http: HttpClient, private router: Router,private gymService:GymService) {
    console.log(this.isMember,this.isAdmin)
  }

  editGym(index: number) {
    this.editIndexMap[index] = true;
  }

  cancelEdit(index: number) {
    delete this.editIndexMap[index];
  }

  saveGym(gym: any, index: number) {


   this.gymService.updateGym(gym).subscribe({
      next: () => {
        alert('✅ Gym updated successfully!');
        delete this.editIndexMap[index];
      },
      error: (err) => {
        alert('❌ Failed to update gym');
        console.error(err);
      }
    });
  }

  deleteGym(gymId: number, index: number) {
    if (!confirm('Are you sure you want to delete this gym?')) return;

       this.gymService.deleteGym(gymId,this.userId).subscribe({
          next: () => {
            alert('✅ Gym and its trainers deleted!');
            this.gyms.splice(index, 1);
          },
          error: (err) => {
            alert('❌ Failed to delete gym');
            console.error(err);
          }
        });
      }



    handleViewClick(gymId: number) {
     // if (this.isAdmin ) {
       this.router.navigate(['/view-people'], {
  state: {
    gymId: gymId,
    userId: this.userId,
    email: this.email,
    isMember: this.isMember,
    isAdmin:this.isAdmin
  }
});
      // } else {
      //   this.router.navigate(['/plans', gymId,this.userId,this.email,this.isMember]);
      // }
    }

    // openPeopleViewClick(gymId: number) {
    //   if (this.isMember) {
    //     this.router.navigate(['/view-people', gymId,this.userId,this.isMember]);
    //   }
    // }
}
