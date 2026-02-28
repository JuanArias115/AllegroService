import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { FirebaseAuthService } from '../core/auth/firebase-auth.service';

interface NavItem {
  label: string;
  path: string;
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
    { label: 'Categories', path: '/categories' },
    { label: 'Products', path: '/products' },
    { label: 'Locations', path: '/locations' },
    { label: 'Reservations', path: '/reservations' }
  ];

  constructor(public readonly auth: FirebaseAuthService) {}

  async logout(): Promise<void> {
    await this.auth.signOut();
  }
}
