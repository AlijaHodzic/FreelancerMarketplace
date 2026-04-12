import { Component, computed, effect, inject, signal } from '@angular/core';
import { NgFor, NgIf } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../../core/auth/auth.service';
import { UserRole } from '../../../../core/models/auth.models';
import { NotificationsService } from '../../../../core/services/notifications.service';

interface HeaderLink {
  label: string;
  path: string;
}

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [NgFor, NgIf, RouterLink, RouterLinkActive],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss'],
})
export class HeaderComponent {
  private readonly authService = inject(AuthService);
  private readonly notificationsService = inject(NotificationsService);

  readonly mobileMenuOpen = signal(false);
  readonly user = this.authService.user;
  readonly role = this.authService.role;
  readonly isAuthenticated = this.authService.isAuthenticated;
  readonly unreadNotifications = this.notificationsService.unreadCount;

  private readonly guestNavLinks: HeaderLink[] = [
    { label: 'Home', path: '/' },
    { label: 'Find Freelancers', path: '/marketplace' },
    { label: 'Browse Jobs', path: '/jobs' },
    { label: 'How It Works', path: '/how-it-works' },
    { label: 'Post a Job', path: '/post-job' },
  ];

  private readonly clientNavLinks: HeaderLink[] = [
    { label: 'Home', path: '/' },
    { label: 'My Jobs', path: '/client-dashboard' },
    { label: 'Saved', path: '/saved-freelancers' },
    { label: 'Inbox', path: '/messages' },
    { label: 'Find Freelancers', path: '/marketplace' },
    { label: 'Post a Job', path: '/post-job' },
  ];

  private readonly adminNavLinks: HeaderLink[] = [
    { label: 'Admin', path: '/admin' },
    { label: 'Users', path: '/admin' },
    { label: 'Projects', path: '/admin' },
    { label: 'Inbox', path: '/messages' },
    { label: 'Notifications', path: '/notifications' },
  ];

  private readonly freelancerNavLinks: HeaderLink[] = [
    { label: 'Home', path: '/' },
    { label: 'Dashboard', path: '/freelancer-dashboard' },
    { label: 'My Applications', path: '/my-applications' },
    { label: 'Inbox', path: '/messages' },
    { label: 'Browse Jobs', path: '/jobs' },
  ];

  readonly navLinks = computed(() => this.resolveNavLinks(this.role()));
  readonly primaryAction = computed(() => this.resolvePrimaryAction(this.role()));

  constructor() {
    effect(() => {
      if (this.isAuthenticated()) {
        this.notificationsService.loadSummary().subscribe();
        return;
      }

      this.notificationsService.clear();
    });
  }

  toggleMobileMenu() {
    this.mobileMenuOpen.update((isOpen) => !isOpen);
  }

  closeMobileMenu() {
    this.mobileMenuOpen.set(false);
  }

  logout() {
    this.authService.logout().subscribe();
    this.closeMobileMenu();
  }

  private resolveNavLinks(role: UserRole | null): HeaderLink[] {
    if (role === 'Freelancer') {
      return this.freelancerNavLinks;
    }

    if (role === 'Admin') {
      return this.adminNavLinks;
    }

    if (role === 'Client') {
      return this.clientNavLinks;
    }

    return this.guestNavLinks;
  }

  private resolvePrimaryAction(role: UserRole | null): HeaderLink | null {
    if (role === 'Client') {
      return { label: 'Post a Job', path: '/post-job' };
    }

    return null;
  }
}
