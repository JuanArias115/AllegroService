import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { FirebaseAuthService } from '../core/auth/firebase-auth.service';
import { I18nService } from '../core/i18n/i18n.service';
import { ThemeService } from '../core/theme/theme.service';
import { IconComponent } from '../shared/components/icon/icon.component';

interface NavItem {
  label: string;
  path: string;
  icon: string;
  requiresInventory?: boolean;
  requiresAdmin?: boolean;
}

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive, TranslateModule, IconComponent],
  templateUrl: './shell.component.html'
})
export class ShellComponent {
  readonly navItems: NavItem[] = [
    { label: 'nav.dashboard', path: '/', icon: 'home' },
    { label: 'nav.units', path: '/units', icon: 'cabin' },
    { label: 'nav.guests', path: '/guests', icon: 'users' },
    { label: 'nav.categories', path: '/categories', icon: 'tag', requiresInventory: true },
    { label: 'nav.products', path: '/products', icon: 'cube', requiresInventory: true },
    { label: 'nav.locations', path: '/locations', icon: 'map', requiresInventory: true },
    { label: 'nav.reservations', path: '/reservations', icon: 'calendar' }
  ];

  constructor(
    public readonly auth: FirebaseAuthService,
    public readonly i18n: I18nService,
    public readonly theme: ThemeService,
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
