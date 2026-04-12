import { Injectable, PLATFORM_ID, computed, inject, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { catchError, map, of, tap } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';
import { AuthResponse, AuthSession, AuthUser, LoginRequest, RegisterRequest, UserRole } from '../models/auth.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly platformId = inject(PLATFORM_ID);
  private readonly storageKey = 'freelance-hub-session';
  private readonly sessionState = signal<AuthSession | null>(this.readStoredSession());

  readonly session = computed(() => this.sessionState());
  readonly user = computed<AuthUser | null>(() => this.sessionState()?.user ?? null);
  readonly role = computed<UserRole | null>(() => this.user()?.role ?? null);
  readonly isAuthenticated = computed(() => !!this.sessionState()?.accessToken);
  readonly accessToken = computed(() => this.sessionState()?.accessToken ?? null);

  login(payload: LoginRequest) {
    return this.http.post<AuthResponse>(`${API_BASE_URL}/auth/login`, payload).pipe(tap((response) => this.storeSession(response)));
  }

  register(payload: RegisterRequest) {
    return this.http.post<AuthResponse>(`${API_BASE_URL}/auth/register`, payload).pipe(tap((response) => this.storeSession(response)));
  }

  logout() {
    const refreshToken = this.sessionState()?.refreshToken;
    if (!refreshToken) {
      this.clearSession();
      return of(void 0);
    }

    return this.http.post<void>(`${API_BASE_URL}/auth/logout`, { refreshToken }).pipe(
      catchError(() => of(void 0)),
      tap(() => this.clearSession()),
      map(() => void 0),
    );
  }

  redirectAfterAuth() {
    const target = this.getDefaultRouteForRole(this.role());
    return this.router.navigateByUrl(target);
  }

  getDefaultRouteForRole(role: UserRole | null) {
    if (role === 'Freelancer') {
      return '/freelancer-dashboard';
    }

    if (role === 'Admin') {
      return '/admin';
    }

    if (role === 'Client') {
      return '/client-dashboard';
    }

    return '/marketplace';
  }

  updateCurrentUser(patch: Partial<AuthUser>) {
    const session = this.sessionState();
    if (!session) {
      return;
    }

    this.storeSession({
      ...session,
      user: {
        ...session.user,
        ...patch,
      },
    });
  }

  private storeSession(session: AuthSession) {
    this.sessionState.set(session);

    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem(this.storageKey, JSON.stringify(session));
    }
  }

  private clearSession() {
    this.sessionState.set(null);

    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem(this.storageKey);
    }

    void this.router.navigateByUrl('/');
  }

  private readStoredSession(): AuthSession | null {
    if (!isPlatformBrowser(this.platformId)) {
      return null;
    }

    const rawSession = localStorage.getItem(this.storageKey);
    if (!rawSession) {
      return null;
    }

    try {
      return JSON.parse(rawSession) as AuthSession;
    } catch {
      localStorage.removeItem(this.storageKey);
      return null;
    }
  }
}
