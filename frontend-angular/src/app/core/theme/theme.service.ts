import { Injectable } from '@angular/core';

export type ThemeMode = 'light' | 'dark';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private static readonly storageKey = 'allegro-theme';

  initialize(): void {
    const stored = localStorage.getItem(ThemeService.storageKey) as ThemeMode | null;
    const systemDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
    this.apply(stored ?? (systemDark ? 'dark' : 'light'));
  }

  toggle(): void {
    this.apply(this.current === 'dark' ? 'light' : 'dark');
  }

  apply(mode: ThemeMode): void {
    const root = document.documentElement;
    root.classList.toggle('dark', mode === 'dark');
    localStorage.setItem(ThemeService.storageKey, mode);
  }

  get current(): ThemeMode {
    return document.documentElement.classList.contains('dark') ? 'dark' : 'light';
  }
}
