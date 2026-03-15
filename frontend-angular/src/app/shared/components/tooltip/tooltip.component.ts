import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { IconComponent } from '../icon/icon.component';

@Component({
  selector: 'app-tooltip',
  standalone: true,
  imports: [CommonModule, IconComponent],
  template: `
    <span class="group relative inline-flex items-center">
      <button
        type="button"
        class="inline-flex h-5 w-5 items-center justify-center rounded-full text-muted transition hover:bg-surface hover:text-accent"
        [attr.aria-label]="text"
      >
        <app-icon name="question" [size]="14"></app-icon>
      </button>

      <span class="pointer-events-none absolute bottom-full left-1/2 z-20 mb-2 hidden w-56 -translate-x-1/2 rounded-lg border border-border bg-panel px-3 py-2 text-left text-xs text-ink shadow-card group-hover:block group-focus-within:block">
        {{ text }}
      </span>
    </span>
  `
})
export class TooltipComponent {
  @Input() text = '';
}
