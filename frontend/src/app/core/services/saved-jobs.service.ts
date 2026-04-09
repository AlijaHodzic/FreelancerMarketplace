import { Injectable, PLATFORM_ID, computed, effect, inject, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { AuthService } from '../auth/auth.service';

@Injectable({ providedIn: 'root' })
export class SavedJobsService {
  private readonly platformId = inject(PLATFORM_ID);
  private readonly authService = inject(AuthService);
  private readonly savedIdsState = signal<string[]>(this.readSavedIds());

  readonly savedIds = computed(() => this.savedIdsState());

  constructor() {
    effect(() => {
      this.authService.user();
      this.savedIdsState.set(this.readSavedIds());
    });
  }

  isSaved(projectId: string) {
    return this.savedIdsState().includes(projectId);
  }

  toggle(projectId: string) {
    const next = this.isSaved(projectId)
      ? this.savedIdsState().filter((id) => id !== projectId)
      : [...this.savedIdsState(), projectId];

    this.savedIdsState.set(next);
    this.persist(next);
  }

  private persist(savedIds: string[]) {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    localStorage.setItem(this.getStorageKey(), JSON.stringify(savedIds));
  }

  private readSavedIds() {
    if (!isPlatformBrowser(this.platformId)) {
      return [];
    }

    const raw = localStorage.getItem(this.getStorageKey());
    if (!raw) {
      return [];
    }

    try {
      const parsed = JSON.parse(raw);
      return Array.isArray(parsed) ? parsed.filter((id): id is string => typeof id === 'string') : [];
    } catch {
      localStorage.removeItem(this.getStorageKey());
      return [];
    }
  }

  private getStorageKey() {
    const userId = this.authService.user()?.id ?? 'guest';
    return `freelance-hub-saved-jobs:${userId}`;
  }
}
