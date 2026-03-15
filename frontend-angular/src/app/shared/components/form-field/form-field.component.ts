import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { TooltipComponent } from '../tooltip/tooltip.component';

@Component({
  selector: 'app-form-field',
  standalone: true,
  imports: [CommonModule, TooltipComponent],
  template: `
    <div class="mb-1 flex items-center gap-2">
      <label class="block text-xs font-semibold uppercase tracking-wide text-muted">{{ label }}</label>
      <app-tooltip *ngIf="tooltip" [text]="tooltip"></app-tooltip>
    </div>
    <ng-content></ng-content>
    <p *ngIf="hint" class="mt-1 text-xs text-muted">{{ hint }}</p>
    <p *ngIf="error" class="mt-1 text-xs text-danger">{{ error }}</p>
  `
})
export class FormFieldComponent {
  @Input() label = '';
  @Input() hint = '';
  @Input() tooltip = '';
  @Input() error = '';
}
