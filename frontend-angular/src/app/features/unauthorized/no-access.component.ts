import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FirebaseAuthService } from '../../core/auth/firebase-auth.service';

@Component({
  selector: 'app-no-access',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="mx-auto mt-16 max-w-lg rounded-xl bg-panel p-8 text-center shadow-card">
      <h1 class="text-2xl font-bold text-danger">Sin acceso</h1>
      <p class="mt-3 text-sm text-muted">{{ message }}</p>
      <p class="mt-3 text-xs text-muted">UID: {{ auth.currentFirebaseUid ?? 'No disponible' }}</p>
      <p class="mt-1 text-xs text-muted">Glamping: {{ auth.glampingId ?? 'No disponible' }}</p>
      <p class="mt-1 text-xs text-muted">Rol: {{ auth.currentRoleLabel }}</p>
      <p class="mt-1 text-xs text-muted">Estado: {{ auth.currentStatusLabel }}</p>

      <div class="mt-5 flex items-center justify-center gap-2">
        <button
          type="button"
          class="inline-block rounded-md border border-slate-300 px-4 py-2 text-sm font-semibold text-slate-800"
          (click)="retry()"
        >
          Reintentar
        </button>
        <a routerLink="/login" class="inline-block rounded-md border border-slate-300 px-4 py-2 text-sm font-semibold text-slate-800">Volver a login</a>
        <button class="inline-block rounded-md bg-slate-900 px-4 py-2 text-sm font-semibold text-white" (click)="logout()">Cerrar sesion</button>
      </div>
    </div>
  `
})
export class NoAccessComponent {
  constructor(
    public readonly auth: FirebaseAuthService,
    private readonly router: Router
  ) {}

  get message(): string {
    const reason = this.auth.noAccessState?.reason;
    if (reason === 'pending' || this.auth.currentStatus === 1) {
      return 'Pendiente de activacion. Tu cuenta existe, pero aun no fue habilitada.';
    }

    if (reason === 'disabled' || this.auth.currentStatus === 3) {
      return 'Cuenta deshabilitada. Contacta a un administrador.';
    }

    if (reason === 'endpoint_missing') {
      return 'No fue posible validar tu acceso en este entorno. Contacta a soporte.';
    }

    return this.auth.noAccessState?.message ?? 'Tu cuenta no esta habilitada todavia. Contacta a un administrador.';
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
