import { Routes } from '@angular/router';
import { MainLayoutComponent } from './layouts/main-layout/main-layout.component';
import { HomeComponent } from './features/landing/pages/home/home.component';
import { MarketplaceComponent } from './features/marketplace/pages/marketplace/marketplace.component';
import { HowItWorksComponent } from './features/how-it-works/pages/how-it-works/how-it-works.component';
import { PostJobComponent } from './features/post-job/pages/post-job/post-job.component';
import { LoginComponent } from './features/auth/pages/login/login.component';
import { RegisterComponent } from './features/auth/pages/register/register.component';
import { JobsComponent } from './features/jobs/pages/jobs/jobs.component';
import { FreelancerProfileComponent } from './features/marketplace/pages/freelancer-profile/freelancer-profile.component';
import { FreelancerDashboardComponent } from './features/dashboard/pages/freelancer-dashboard/freelancer-dashboard.component';
import { FreelancerApplicationsComponent } from './features/dashboard/pages/freelancer-applications/freelancer-applications.component';
import { ClientDashboardComponent } from './features/dashboard/pages/client-dashboard/client-dashboard.component';
import { MessagesComponent } from './features/messages/pages/messages/messages.component';
import { NotificationsComponent } from './features/notifications/pages/notifications/notifications.component';
import { FavoriteFreelancersComponent } from './features/favorites/pages/favorite-freelancers/favorite-freelancers.component';
import { AdminDashboardComponent } from './features/admin/pages/admin-dashboard/admin-dashboard.component';
import {
  adminOnlyGuard,
  authenticatedOnlyGuard,
  clientOnlyGuard,
  freelancerOnlyGuard,
  freelancerOrGuestGuard,
  guestOnlyGuard,
  marketplaceAccessGuard,
} from './core/auth/auth.guards';

export const routes: Routes = [
  {
    path: '',
    component: MainLayoutComponent,
    children: [
      { path: '', component: HomeComponent },
      { path: 'marketplace', component: MarketplaceComponent, canActivate: [marketplaceAccessGuard] },
      { path: 'freelancers/:slug', component: FreelancerProfileComponent, canActivate: [marketplaceAccessGuard] },
      { path: 'freelancer-dashboard', component: FreelancerDashboardComponent, canActivate: [freelancerOnlyGuard] },
      { path: 'my-applications', component: FreelancerApplicationsComponent, canActivate: [freelancerOnlyGuard] },
      { path: 'client-dashboard', component: ClientDashboardComponent, canActivate: [clientOnlyGuard] },
      { path: 'saved-freelancers', component: FavoriteFreelancersComponent, canActivate: [clientOnlyGuard] },
      { path: 'messages', component: MessagesComponent, canActivate: [authenticatedOnlyGuard] },
      { path: 'notifications', component: NotificationsComponent, canActivate: [authenticatedOnlyGuard] },
      { path: 'admin', component: AdminDashboardComponent, canActivate: [adminOnlyGuard] },
      { path: 'jobs', component: JobsComponent, canActivate: [freelancerOrGuestGuard] },
      { path: 'how-it-works', component: HowItWorksComponent },
      { path: 'post-job', component: PostJobComponent, canActivate: [clientOnlyGuard] },
      { path: 'login', component: LoginComponent, canActivate: [guestOnlyGuard] },
      { path: 'register', component: RegisterComponent, canActivate: [guestOnlyGuard] },
    ],
  },
  { path: '**', redirectTo: '' },
];
