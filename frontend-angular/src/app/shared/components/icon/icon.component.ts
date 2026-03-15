import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

type IconName =
  | 'home'
  | 'cabin'
  | 'users'
  | 'cube'
  | 'tag'
  | 'map'
  | 'calendar'
  | 'plus'
  | 'pencil'
  | 'trash'
  | 'credit-card'
  | 'arrow-right-circle'
  | 'moon'
  | 'sun'
  | 'language'
  | 'chat'
  | 'receipt'
  | 'shopping-bag'
  | 'check'
  | 'question';

@Component({
  selector: 'app-icon',
  standalone: true,
  imports: [CommonModule],
  template: `
    <svg
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      stroke-linecap="round"
      stroke-linejoin="round"
      [attr.stroke-width]="strokeWidth"
      [style.width.rem]="size / 16"
      [style.height.rem]="size / 16"
      aria-hidden="true"
    >
      <ng-container [ngSwitch]="name">
        <ng-container *ngSwitchCase="'home'"><path d="M3 10.75 12 3l9 7.75"/><path d="M5 9.75V21h14V9.75"/><path d="M9 21v-6h6v6"/></ng-container>
        <ng-container *ngSwitchCase="'cabin'"><path d="M4 20h16"/><path d="M6 20V9l6-4 6 4v11"/><path d="M9 14h6"/><path d="M10 20v-4h4v4"/></ng-container>
        <ng-container *ngSwitchCase="'users'"><path d="M16 21v-2a4 4 0 0 0-4-4H7a4 4 0 0 0-4 4v2"/><circle cx="9.5" cy="7" r="3"/><path d="M20 21v-2a4 4 0 0 0-3-3.87"/><path d="M16.5 4.13a3 3 0 0 1 0 5.74"/></ng-container>
        <ng-container *ngSwitchCase="'cube'"><path d="m12 2 8 4.5v9L12 20 4 15.5v-9L12 2Z"/><path d="M12 20V11"/><path d="m20 6.5-8 4.5-8-4.5"/></ng-container>
        <ng-container *ngSwitchCase="'tag'"><path d="m20 10.5-8.5 8.5L3 10.5V3h7.5L20 10.5Z"/><circle cx="7.5" cy="7.5" r="1"/></ng-container>
        <ng-container *ngSwitchCase="'map'"><path d="m3 6 6-2 6 2 6-2v14l-6 2-6-2-6 2V6Z"/><path d="M9 4v14"/><path d="M15 6v14"/></ng-container>
        <ng-container *ngSwitchCase="'calendar'"><rect x="3" y="5" width="18" height="16" rx="2"/><path d="M16 3v4"/><path d="M8 3v4"/><path d="M3 11h18"/></ng-container>
        <ng-container *ngSwitchCase="'plus'"><path d="M12 5v14"/><path d="M5 12h14"/></ng-container>
        <ng-container *ngSwitchCase="'pencil'"><path d="m4 20 4-.8L19.2 8a2.1 2.1 0 0 0-3-3L5 16.2 4 20Z"/><path d="m13.5 5.5 5 5"/></ng-container>
        <ng-container *ngSwitchCase="'trash'"><path d="M3 6h18"/><path d="M8 6V4h8v2"/><path d="m19 6-1 14H6L5 6"/><path d="M10 10v6"/><path d="M14 10v6"/></ng-container>
        <ng-container *ngSwitchCase="'credit-card'"><rect x="3" y="5" width="18" height="14" rx="2"/><path d="M3 10h18"/><path d="M7 15h3"/></ng-container>
        <ng-container *ngSwitchCase="'arrow-right-circle'"><circle cx="12" cy="12" r="9"/><path d="M10 8l4 4-4 4"/><path d="M8 12h6"/></ng-container>
        <ng-container *ngSwitchCase="'moon'"><path d="M21 12.8A9 9 0 1 1 11.2 3 7 7 0 0 0 21 12.8Z"/></ng-container>
        <ng-container *ngSwitchCase="'sun'"><circle cx="12" cy="12" r="4"/><path d="M12 2v2.5"/><path d="M12 19.5V22"/><path d="m4.9 4.9 1.8 1.8"/><path d="m17.3 17.3 1.8 1.8"/><path d="M2 12h2.5"/><path d="M19.5 12H22"/><path d="m4.9 19.1 1.8-1.8"/><path d="m17.3 6.7 1.8-1.8"/></ng-container>
        <ng-container *ngSwitchCase="'language'"><path d="M4 5h7"/><path d="M7.5 5c0 6-3 10-3 10"/><path d="M7.5 5c0 2 1 5 3 7"/><path d="M13 19h7"/><path d="m16 6 4 13"/><path d="m18.5 14h-5"/></ng-container>
        <ng-container *ngSwitchCase="'chat'"><path d="M7 18 3 21V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2v11a2 2 0 0 1-2 2H7Z"/></ng-container>
        <ng-container *ngSwitchCase="'receipt'"><path d="M6 3h12v18l-2-1.5L14 21l-2-1.5L10 21l-2-1.5L6 21V3Z"/><path d="M9 8h6"/><path d="M9 12h6"/><path d="M9 16h4"/></ng-container>
        <ng-container *ngSwitchCase="'shopping-bag'"><path d="M6 8h12l-1 12H7L6 8Z"/><path d="M9 8a3 3 0 1 1 6 0"/></ng-container>
        <ng-container *ngSwitchCase="'check'"><path d="m5 12 4 4L19 6"/></ng-container>
        <ng-container *ngSwitchDefault><circle cx="12" cy="12" r="9"/><path d="M12 8v5"/><path d="M12 16h.01"/></ng-container>
      </ng-container>
    </svg>
  `
})
export class IconComponent {
  @Input() name: IconName | string = 'question';
  @Input() size = 18;
  @Input() strokeWidth = 1.8;
}
