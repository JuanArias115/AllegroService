import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { FirebaseAuthService } from '../../core/auth/firebase-auth.service';
import { IconComponent } from '../../shared/components/icon/icon.component';

@Component({
  selector: 'app-no-access',
  standalone: true,
  imports: [CommonModule, RouterLink, TranslateModule, IconComponent],
  template: `
    <div class="mx-auto mt-16 max-w-lg rounded-2xl border border-border bg-panel p-8 text-center shadow-card">
      <div class="mx-auto flex h-14 w-14 items-center justify-center rounded-2xl bg-danger/10 text-danger">
        <app-icon name="question" [size]="24"></app-icon>
      </div>
      <h1 class="mt-4 text-2xl font-bold text-danger">{{ 'noaccess.title' | translate }}</h1>
      <p class="mt-3 text-sm text-muted">{{ message }}</p>
      <p class="mt-3 text-xs text-muted">UID: {{ auth.currentFirebaseUid ?? 'No disponible' }}</p>
      <p class="mt-1 text-xs text-muted">Glamping: {{ auth.glampingId ?? 'No disponible' }}</p>
      <p class="mt-1 text-xs text-muted">Rol: {{ auth.currentRoleLabel }}</p>
      <p class="mt-1 text-xs text-muted">Estado: {{ auth.currentStatusLabel }}</p>

      <div class="mt-5 flex items-center justify-center gap-2">
        <button
          type="button"
          class="inline-block rounded-xl border border-border px-4 py-2 text-sm font-semibold text-ink"
          (click)="retry()"
        >
          {{ 'noaccess.retry' | translate }}
        </button>
        <a routerLink="/login" class="inline-block rounded-xl border border-border px-4 py-2 text-sm font-semibold text-ink">{{ 'common.back' | translate }}</a>
        <button class="inline-block rounded-xl bg-primary px-4 py-2 text-sm font-semibold text-white" (click)="logout()">{{ 'common.logout' | translate }}</button>
      </div>
    </div>
  `
})
export class NoAccessComponent {
  constructor(
    public readonly auth: FirebaseAuthService,
    private readonly router: Router,
    private readonly translate: TranslateService
  ) {}

  get message(): string {
    const reason = this.auth.noAccessState?.reason;
    if (reason === 'pending' || this.auth.currentStatus === 1) {
      return this.translate.instant('noaccess.pending');
    }

    if (reason === 'disabled' || this.auth.currentStatus === 3) {
      return this.translate.instant('noaccess.disabled');
    }

    if (reason === 'endpoint_missing') {
      return this.translate.instant('noaccess.generic');
    }

    return this.auth.noAccessState?.message ?? this.translate.instant('noaccess.generic');
  }

  async retry(): Promise<void> {
    try {
      await this.auth.loadUserTenant(true);
      if (this.auth.isActiveSession()) {
        await this.router.navigate(['/']);
      }
    } catch {
      // Keep no-access view with latest status message.
    }
  }

  async logout(): Promise<void> {
    await this.auth.signOut();
    await this.router.navigate(['/login']);
  }
}
