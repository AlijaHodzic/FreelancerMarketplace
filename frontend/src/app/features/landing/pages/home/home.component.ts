import { Component } from '@angular/core';
import { LandingPageComponent } from '../landing-page/landing-page.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [LandingPageComponent],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
})
export class HomeComponent {}
