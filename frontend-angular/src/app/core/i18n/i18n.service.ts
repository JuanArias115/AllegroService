import { Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

@Injectable({ providedIn: 'root' })
export class I18nService {
  private static readonly storageKey = 'allegro-language';
  readonly supported = ['es', 'en'] as const;

  constructor(private readonly translate: TranslateService) {}

  initialize(): void {
    this.translate.addLangs([...this.supported]);
    this.translate.setDefaultLang('es');

    const stored = localStorage.getItem(I18nService.storageKey);
    const browser = navigator.language.toLowerCase().startsWith('en') ? 'en' : 'es';
    const nextLang = stored && this.supported.includes(stored as 'es' | 'en') ? stored : browser;

    this.translate.use(nextLang);
  }

  use(lang: 'es' | 'en'): void {
    localStorage.setItem(I18nService.storageKey, lang);
    this.translate.use(lang);
  }

  get current(): 'es' | 'en' {
    const current = this.translate.currentLang || this.translate.defaultLang || 'es';
    return current === 'en' ? 'en' : 'es';
  }
}
