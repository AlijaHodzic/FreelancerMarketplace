import { DOCUMENT, isPlatformBrowser } from '@angular/common';
import { Injectable, PLATFORM_ID, effect, inject, signal } from '@angular/core';

const THEME_STORAGE_KEY = 'freelancehub-theme';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly document = inject(DOCUMENT);
  private readonly platformId = inject(PLATFORM_ID);
  private readonly isBrowser = isPlatformBrowser(this.platformId);

  readonly isDarkMode = signal(false);

  constructor() {
    if (this.isBrowser) {
      const storedTheme = window.localStorage.getItem(THEME_STORAGE_KEY);
      const prefersDark = window.matchMedia?.('(prefers-color-scheme: dark)').matches ?? false;
      this.isDarkMode.set(storedTheme ? storedTheme === 'dark' : prefersDark);
    }

    effect(() => {
      const isDark = this.isDarkMode();
      const root = this.document?.documentElement;

      if (!root) {
        return;
      }

      root.classList.toggle('dark', isDark);
      root.style.colorScheme = isDark ? 'dark' : 'light';

      if (this.isBrowser) {
        window.localStorage.setItem(THEME_STORAGE_KEY, isDark ? 'dark' : 'light');
      }
    });
  }

  toggleTheme() {
    this.isDarkMode.update((value) => !value);
  }
}
