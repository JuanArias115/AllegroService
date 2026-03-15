import { Component } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [TranslateModule],
  template: `
    <div class="space-y-4">
      <h2 class="text-2xl font-bold">{{ 'dashboard.title' | translate }}</h2>
      <p class="text-sm text-muted">{{ 'dashboard.subtitle' | translate }}</p>
      <div class="grid gap-4 md:grid-cols-3">
        <article class="rounded-xl bg-panel p-4 shadow-card">
          <p class="text-xs uppercase tracking-wide text-muted">{{ 'dashboard.frontdesk' | translate }}</p>
          <h3 class="mt-2 text-lg font-semibold">Reservations & Check-in</h3>
          <p class="mt-1 text-sm text-muted">Gestiona reservas y entradas desde el modulo Reservations.</p>
        </article>
        <article class="rounded-xl bg-panel p-4 shadow-card">
          <p class="text-xs uppercase tracking-wide text-muted">{{ 'dashboard.folio' | translate }}</p>
          <h3 class="mt-2 text-lg font-semibold">Charges & Payments</h3>
          <p class="mt-1 text-sm text-muted">Controla cargos y pagos por estadia para cerrar sin saldo.</p>
        </article>
        <article class="rounded-xl bg-panel p-4 shadow-card">
          <p class="text-xs uppercase tracking-wide text-muted">{{ 'dashboard.inventory' | translate }}</p>
          <h3 class="mt-2 text-lg font-semibold">Products & Stock</h3>
          <p class="mt-1 text-sm text-muted">Administra catalogo, ubicaciones y consumos con stock.</p>
        </article>
      </div>
    </div>
  `
})
export class DashboardComponent {}
