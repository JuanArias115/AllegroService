import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs/operators';
import { DataTableComponent, TableColumn } from '../../shared/components/data-table/data-table.component';
import { ModalComponent } from '../../shared/components/modal/modal.component';
import { FormFieldComponent } from '../../shared/components/form-field/form-field.component';
import { Unit } from '../../core/models/domain.model';
import { UnitsApi } from './units.api';
import { ToastService } from '../../core/ui/toast.service';
import { UNIT_STATUS_OPTIONS, labelOf } from '../../core/models/enums';

@Component({
  selector: 'app-units-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DataTableComponent, ModalComponent, FormFieldComponent],
  templateUrl: './units-page.component.html'
})
export class UnitsPageComponent implements OnInit {
  readonly columns: TableColumn[] = [
    { key: 'name', label: 'Name' },
    { key: 'type', label: 'Type' },
    { key: 'capacity', label: 'Capacity' },
    { key: 'statusLabel', label: 'Status' }
  ];

  readonly statusOptions = UNIT_STATUS_OPTIONS;

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required]],
    type: ['', [Validators.required]],
    capacity: [1, [Validators.required, Validators.min(1)]],
    status: [1, [Validators.required]]
  });

  units: Unit[] = [];
  rows: Record<string, unknown>[] = [];
  selectedId: string | null = null;
  loading = false;
  saving = false;
  modalOpen = false;

  page = 1;
  pageSize = 10;
  total = 0;
  search = '';

  constructor(
    private readonly fb: FormBuilder,
    private readonly api: UnitsApi,
    private readonly toast: ToastService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.api
      .list({ page: this.page, pageSize: this.pageSize, search: this.search, sort: 'name' })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe((response) => {
        this.units = response.items;
        this.rows = response.items.map((unit) => ({
          ...unit,
          statusLabel: labelOf(this.statusOptions, unit.status)
        }));
        this.total = response.total;
      });
  }

  onSearch(value: string): void {
    this.search = value;
    this.page = 1;
    this.load();
  }

  onPageChange(page: number): void {
    this.page = page;
    this.load();
  }

  openCreate(): void {
    this.selectedId = null;
    this.form.reset({ name: '', type: '', capacity: 1, status: 1 });
    this.modalOpen = true;
  }

  openEdit(row: Record<string, unknown>): void {
    const id = String(row['id']);
    const found = this.units.find((unit) => unit.id === id);
    if (!found) {
      return;
    }

    this.selectedId = found.id;
    this.form.reset({
      name: found.name,
      type: found.type,
      capacity: found.capacity,
      status: found.status
    });
    this.modalOpen = true;
  }

  remove(row: Record<string, unknown>): void {
    const id = String(row['id']);
    if (!confirm('Delete this unit?')) {
      return;
    }

    this.api.remove(id).subscribe(() => {
      this.toast.success('Unit removed.');
      this.load();
    });
  }

  submit(): void {
    if (this.form.invalid || this.saving) {
      this.form.markAllAsTouched();
      return;
    }

    const payload = {
      name: this.form.controls.name.value,
      type: this.form.controls.type.value,
      capacity: this.form.controls.capacity.value,
      status: this.form.controls.status.value
    };

    this.saving = true;

    const request$ = this.selectedId
      ? this.api.update(this.selectedId, payload)
      : this.api.create({ name: payload.name, type: payload.type, capacity: payload.capacity });

    request$.pipe(finalize(() => (this.saving = false))).subscribe(() => {
      this.toast.success(this.selectedId ? 'Unit updated.' : 'Unit created.');
      this.modalOpen = false;
      this.load();
    });
  }
}
