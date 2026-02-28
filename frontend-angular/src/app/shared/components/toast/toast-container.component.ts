import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { ToastService } from '../../../core/ui/toast.service';

@Component({
  selector: 'app-toast-container',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="fixed right-4 top-4 z-[100] space-y-2">
      <div
        *ngFor="let message of toast.messages$ | async"
        class="min-w-72 rounded-lg border px-4 py-3 text-sm shadow-card"
        [class.border-emerald-300]="message.type === 'success'"
        [class.bg-emerald-50]="message.type === 'success'"
        [class.border-red-300]="message.type === 'error'"
        [class.bg-red-50]="message.type === 'error'"
        [class.border-slate-300]="message.type === 'info'"
        [class.bg-slate-50]="message.type === 'info'"
      >
        <div class="flex items-start justify-between gap-3">
          <span>{{ message.text }}</span>
          <button class="text-xs text-slate-600" (click)="toast.dismiss(message.id)">x</button>
        </div>
      </div>
    </div>
  `
})
export class ToastContainerComponent {
  constructor(public readonly toast: ToastService) {}
}
