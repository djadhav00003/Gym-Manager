import { Component, Input, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Member } from '../../models/user.model';

@Component({
  selector: 'app-member-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css'],
})
export class MemberListComponent {
  @Input() members: Member[] = [];
  @Input() currentUserId?: number | null;
  @Input() isMember: boolean = false;
  @Input() isAdmin: boolean = false;

  // values used in template
  public userMember?: Member | null;
  public otherCount = 0;

  ngOnChanges(changes: SimpleChanges) {
    this.recompute();
  }

  private recompute() {
    const list = this.members || [];

    // try to find first member that belongs to the current user
    if (this.currentUserId != null) {
      // adjust property name if your Member model uses a different key (e.g. UserId)
      this.userMember =
        list.find((m) => (m as any).userId === this.currentUserId) ?? null;
    } else {
      this.userMember = null;
    }

    // otherCount is total minus the user row (if present)
    this.otherCount = list.length - (this.userMember ? 1 : 0);
    if (this.otherCount < 0) this.otherCount = 0;
  }
}
