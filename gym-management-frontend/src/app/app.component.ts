import { Component, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterModule],
  template: `
    <router-outlet></router-outlet>
  `,
})
export class AppComponent implements OnInit {
  constructor(private auth: AuthService) {}

  ngOnInit() {
    // Silent refresh on app load — sets in-memory token if refresh cookie valid
    this.auth.initialize().subscribe(ok => {
      console.log('Auth init:', ok);
    });
  }
}
