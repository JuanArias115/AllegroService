import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-form-field',
  standalone: true,
  imports: [CommonModule],
  template: `
    <label class="mb-1 block text-xs font-semibold uppercase tracking-wide text-slate-600">{{ label }}</label>
    <ng-content></ng-content>
    <p *ngIf="error" class="mt-1 text-xs text-danger">{{ error }}</p>
  `
})
export class FormFieldComponent {
  @Input() label = '';
  @Input() error = '';
}
