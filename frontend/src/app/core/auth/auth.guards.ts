import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from './auth.service';

export const guestOnlyGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree([authService.role() === 'Freelancer' ? '/jobs' : '/marketplace']);
};

export const clientOnlyGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    return router.createUrlTree(['/login']);
  }

  if (authService.role() === 'Freelancer') {
    return router.createUrlTree(['/freelancer-dashboard']);
  }

  return true;
};

export const freelancerOnlyGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    return router.createUrlTree(['/login']);
  }

  if (authService.role() !== 'Freelancer') {
    return router.createUrlTree(['/marketplace']);
  }

  return true;
};

export const freelancerOrGuestGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated() || authService.role() === 'Freelancer') {
    return true;
  }

  return router.createUrlTree(['/marketplace']);
};

export const marketplaceAccessGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.role() === 'Freelancer') {
    return router.createUrlTree(['/freelancer-dashboard']);
  }

  return true;
};
