import { Component } from '@angular/core';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  template: `
    <div class="space-y-4">
      <h2 class="text-2xl font-bold">Dashboard</h2>
      <p class="text-sm text-muted">Operacion diaria del glamping.</p>
      <div class="grid gap-4 md:grid-cols-3">
        <article class="rounded-xl bg-panel p-4 shadow-card">
          <p class="text-xs uppercase tracking-wide text-muted">Front Desk</p>
          <h3 class="mt-2 text-lg font-semibold">Reservations & Check-in</h3>
          <p class="mt-1 text-sm text-muted">Gestiona reservas y entradas desde el modulo Reservations.</p>
        </article>
        <article class="rounded-xl bg-panel p-4 shadow-card">
          <p class="text-xs uppercase tracking-wide text-muted">Folio</p>
          <h3 class="mt-2 text-lg font-semibold">Charges & Payments</h3>
          <p class="mt-1 text-sm text-muted">Controla cargos y pagos por estadia para cerrar sin saldo.</p>
        </article>
        <article class="rounded-xl bg-panel p-4 shadow-card">
          <p class="text-xs uppercase tracking-wide text-muted">Inventory</p>
          <h3 class="mt-2 text-lg font-semibold">Products & Stock</h3>
          <p class="mt-1 text-sm text-muted">Administra catalogo, ubicaciones y consumos con stock.</p>
        </article>
      </div>
    </div>
  `
})
export class DashboardComponent {}
