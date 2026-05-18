import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthService } from './core/auth/auth.service';
import { ThemeService } from './core/services/theme.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  private readonly authService = inject(AuthService);
  private readonly themeService = inject(ThemeService);
  title = 'freelance-hub';

  constructor() {
    this.authService.bootstrapDemoClientSession();
  }
}
