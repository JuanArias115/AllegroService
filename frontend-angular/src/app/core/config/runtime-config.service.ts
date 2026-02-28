import { Injectable } from '@angular/core';
import { RuntimeConfig } from './runtime-config.model';

@Injectable({ providedIn: 'root' })
export class RuntimeConfigService {
  private config: RuntimeConfig | null = null;

  async load(): Promise<void> {
    const response = await fetch('/assets/config.json', { cache: 'no-store' });
    if (!response.ok) {
      throw new Error('Unable to load runtime configuration.');
    }

    const parsed = (await response.json()) as RuntimeConfig;
    this.config = parsed;
  }

  get value(): RuntimeConfig {
    if (!this.config) {
      throw new Error('Runtime config has not been loaded.');
    }

    return this.config;
  }
}
