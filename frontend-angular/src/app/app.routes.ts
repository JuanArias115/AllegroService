import { Routes } from '@angular/router';
import { ShellComponent } from './layout/shell.component';
import { authGuard, loginGuard } from './core/auth/auth.guard';
import { LoginComponent } from './features/login/login.component';
import { NoAccessComponent } from './features/unauthorized/no-access.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { UnitsPageComponent } from './features/units/units-page.component';
import { GuestsPageComponent } from './features/guests/guests-page.component';
import { CategoriesPageComponent } from './features/categories/categories-page.component';
import { ProductsPageComponent } from './features/products/products-page.component';
import { LocationsPageComponent } from './features/locations/locations-page.component';
import { ReservationsPageComponent } from './features/reservations/reservations-page.component';
import { ReservationDetailComponent } from './features/reservations/reservation-detail.component';
import { StayDetailComponent } from './features/stays/stay-detail.component';

export const routes: Routes = [
  {
    path: 'login',
    component: LoginComponent,
    canActivate: [loginGuard]
  },
  {
    path: 'no-access',
    component: NoAccessComponent
  },
  {
    path: '',
    component: ShellComponent,
    canActivate: [authGuard],
    children: [
      { path: '', component: DashboardComponent },
      { path: 'units', component: UnitsPageComponent },
      { path: 'guests', component: GuestsPageComponent },
      { path: 'categories', component: CategoriesPageComponent },
      { path: 'products', component: ProductsPageComponent },
      { path: 'locations', component: LocationsPageComponent },
      { path: 'reservations', component: ReservationsPageComponent },
      { path: 'reservations/:id', component: ReservationDetailComponent },
      { path: 'stays/:id', component: StayDetailComponent }
    ]
  },
  {
    path: '**',
    redirectTo: ''
  }
];
