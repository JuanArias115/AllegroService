import { Component } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FirebaseAuthService } from '../../core/auth/firebase-auth.service';

@Component({
  selector: 'app-no-access',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="mx-auto mt-16 max-w-lg rounded-xl bg-panel p-8 text-center shadow-card">
      <h1 class="text-2xl font-bold text-danger">Sin acceso</h1>
      <p class="mt-3 text-sm text-muted">
        Tu cuenta no tiene el claim <code>glamping_id</code> valido. Pide al administrador asignar claims y vuelve a iniciar sesion.
      </p>
      <p class="mt-3 text-xs text-muted">glamping_id detectado: {{ auth.glampingId ?? 'No disponible' }}</p>
      <p class="mt-1 text-xs text-muted">role detectado: {{ auth.currentRole ?? 'No asignado' }}</p>

      <div class="mt-5 flex items-center justify-center gap-2">
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

  async logout(): Promise<void> {
    await this.auth.signOut();
    await this.router.navigate(['/login']);
  }
}
