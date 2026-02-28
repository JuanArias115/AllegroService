import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

export interface TableColumn {
  key: string;
  label: string;
}

@Component({
  selector: 'app-data-table',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './data-table.component.html'
})
export class DataTableComponent {
  @Input() columns: TableColumn[] = [];
  @Input() rows: Record<string, unknown>[] = [];
  @Input() loading = false;
  @Input() page = 1;
  @Input() pageSize = 20;
  @Input() total = 0;
  @Input() showActions = true;

  @Output() pageChange = new EventEmitter<number>();
  @Output() edit = new EventEmitter<Record<string, unknown>>();
  @Output() remove = new EventEmitter<Record<string, unknown>>();

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.total / this.pageSize));
  }

  toDisplay(value: unknown): string {
    if (value === null || value === undefined) {
      return '-';
    }

    if (typeof value === 'boolean') {
      return value ? 'Yes' : 'No';
    }

    return String(value);
  }

  nextPage(): void {
    if (this.page < this.totalPages) {
      this.pageChange.emit(this.page + 1);
    }
  }

  previousPage(): void {
    if (this.page > 1) {
      this.pageChange.emit(this.page - 1);
    }
  }
}
