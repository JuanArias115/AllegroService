import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FirebaseAuthService } from '../../core/auth/firebase-auth.service';

@Component({
  selector: 'app-no-access',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="mx-auto mt-16 max-w-lg rounded-xl bg-panel p-8 text-center shadow-card">
      <h1 class="text-2xl font-bold text-danger">Sin acceso</h1>
      <p class="mt-3 text-sm text-muted">
        El token no contiene el claim <code>glamping_id</code> valido, por lo que no se puede operar en modo multi-tenant.
      </p>
      <p class="mt-3 text-xs text-muted">glamping_id detectado: {{ auth.glampingId ?? 'No disponible' }}</p>
      <a routerLink="/login" class="mt-5 inline-block rounded-md bg-slate-900 px-4 py-2 text-sm font-semibold text-white">Volver a login</a>
    </div>
  `
})
export class NoAccessComponent {
  constructor(public readonly auth: FirebaseAuthService) {}
}
