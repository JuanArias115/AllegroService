import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs/operators';
import { DataTableComponent, TableColumn } from '../../shared/components/data-table/data-table.component';
import { ModalComponent } from '../../shared/components/modal/modal.component';
import { FormFieldComponent } from '../../shared/components/form-field/form-field.component';
import { ToastService } from '../../core/ui/toast.service';
import { LOCATION_TYPE_OPTIONS, labelOf } from '../../core/models/enums';
import { Location, Unit } from '../../core/models/domain.model';
import { LocationsApi } from './locations.api';
import { UnitsApi } from '../units/units.api';

@Component({
  selector: 'app-locations-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DataTableComponent, ModalComponent, FormFieldComponent],
  templateUrl: './locations-page.component.html'
})
export class LocationsPageComponent implements OnInit {
  readonly columns: TableColumn[] = [
    { key: 'name', label: 'Name' },
    { key: 'typeLabel', label: 'Type' },
    { key: 'unitLabel', label: 'Unit' }
  ];

  readonly locationTypeOptions = LOCATION_TYPE_OPTIONS;

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required]],
    type: [1, [Validators.required]],
    unitId: ['']
  });

  units: Unit[] = [];
  locations: Location[] = [];
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
    private readonly api: LocationsApi,
    private readonly unitsApi: UnitsApi,
    private readonly toast: ToastService
  ) {}

  ngOnInit(): void {
    this.loadUnits();
    this.load();
  }

  loadUnits(): void {
    this.unitsApi.list({ page: 1, pageSize: 200, sort: 'name' }).subscribe((response) => {
      this.units = response.items;
    });
  }

  load(): void {
    this.loading = true;
    this.api
      .list({ page: this.page, pageSize: this.pageSize, search: this.search, sort: 'name' })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe((response) => {
        this.locations = response.items;
        this.rows = response.items.map((location) => ({
          ...location,
          typeLabel: labelOf(this.locationTypeOptions, location.type),
          unitLabel: this.units.find((unit) => unit.id === location.unitId)?.name ?? '-'
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
    this.form.reset({ name: '', type: 1, unitId: '' });
    this.modalOpen = true;
  }

  openEdit(row: Record<string, unknown>): void {
    const id = String(row['id']);
    const found = this.locations.find((location) => location.id === id);
    if (!found) {
      return;
    }

    this.selectedId = found.id;
    this.form.reset({
      name: found.name,
      type: found.type,
      unitId: found.unitId ?? ''
    });
    this.modalOpen = true;
  }

  remove(row: Record<string, unknown>): void {
    const id = String(row['id']);
    if (!confirm('Delete this location?')) {
      return;
    }

    this.api.remove(id).subscribe(() => {
      this.toast.success('Location removed.');
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
      type: Number(this.form.controls.type.value),
      unitId: this.form.controls.unitId.value || null
    };

    this.saving = true;
    const request$ = this.selectedId ? this.api.update(this.selectedId, payload) : this.api.create(payload);

    request$.pipe(finalize(() => (this.saving = false))).subscribe(() => {
      this.toast.success(this.selectedId ? 'Location updated.' : 'Location created.');
      this.modalOpen = false;
      this.load();
    });
  }
}
