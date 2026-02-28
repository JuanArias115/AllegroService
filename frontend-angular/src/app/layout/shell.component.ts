import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { FirebaseAuthService } from '../core/auth/firebase-auth.service';

interface NavItem {
  label: string;
  path: string;
  requiresInventory?: boolean;
  requiresAdmin?: boolean;
}

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './shell.component.html'
})
export class ShellComponent {
  readonly navItems: NavItem[] = [
    { label: 'Dashboard', path: '/' },
    { label: 'Units', path: '/units' },
    { label: 'Guests', path: '/guests' },
    { label: 'Categories', path: '/categories', requiresInventory: true },
    { label: 'Products', path: '/products', requiresInventory: true },
    { label: 'Locations', path: '/locations', requiresInventory: true },
    { label: 'Reservations', path: '/reservations' }
  ];

  constructor(
    public readonly auth: FirebaseAuthService,
    private readonly router: Router
  ) {}

  canShow(item: NavItem): boolean {
    if (item.requiresAdmin) {
      return this.auth.canManageUsers();
    }

    if (item.requiresInventory) {
      return this.auth.canViewInventory();
    }

    return true;
  }

  async logout(): Promise<void> {
    await this.auth.signOut();
    await this.router.navigate(['/login']);
  }
}
