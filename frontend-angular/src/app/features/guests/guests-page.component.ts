import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs/operators';
import { GuestsApi } from './guests.api';
import { Guest } from '../../core/models/domain.model';
import { ToastService } from '../../core/ui/toast.service';
import { DataTableComponent, TableColumn } from '../../shared/components/data-table/data-table.component';
import { ModalComponent } from '../../shared/components/modal/modal.component';
import { FormFieldComponent } from '../../shared/components/form-field/form-field.component';

@Component({
  selector: 'app-guests-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DataTableComponent, ModalComponent, FormFieldComponent],
  templateUrl: './guests-page.component.html'
})
export class GuestsPageComponent implements OnInit {
  readonly columns: TableColumn[] = [
    { key: 'fullName', label: 'Guest' },
    { key: 'documentId', label: 'Document' },
    { key: 'phone', label: 'Phone' },
    { key: 'email', label: 'Email' }
  ];

  readonly form = this.fb.nonNullable.group({
    fullName: ['', [Validators.required]],
    documentId: [''],
    phone: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]]
  });

  guests: Guest[] = [];
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
    private readonly api: GuestsApi,
    private readonly toast: ToastService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.api
      .list({ page: this.page, pageSize: this.pageSize, search: this.search, sort: 'fullName' })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe((response) => {
        this.guests = response.items;
        this.rows = response.items.map((guest) => ({ ...guest }));
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
    this.form.reset({ fullName: '', documentId: '', phone: '', email: '' });
    this.modalOpen = true;
  }

  openEdit(row: Record<string, unknown>): void {
    const id = String(row['id']);
    const found = this.guests.find((guest) => guest.id === id);
    if (!found) {
      return;
    }

    this.selectedId = found.id;
    this.form.reset({
      fullName: found.fullName,
      documentId: found.documentId ?? '',
      phone: found.phone,
      email: found.email
    });
    this.modalOpen = true;
  }

  remove(row: Record<string, unknown>): void {
    const id = String(row['id']);
    if (!confirm('Delete this guest?')) {
      return;
    }

    this.api.remove(id).subscribe(() => {
      this.toast.success('Guest removed.');
      this.load();
    });
  }

  submit(): void {
    if (this.form.invalid || this.saving) {
      this.form.markAllAsTouched();
      return;
    }

    const payload = {
      fullName: this.form.controls.fullName.value,
      documentId: this.form.controls.documentId.value || null,
      phone: this.form.controls.phone.value,
      email: this.form.controls.email.value
    };

    this.saving = true;
    const request$ = this.selectedId ? this.api.update(this.selectedId, payload) : this.api.create(payload);

    request$.pipe(finalize(() => (this.saving = false))).subscribe(() => {
      this.toast.success(this.selectedId ? 'Guest updated.' : 'Guest created.');
      this.modalOpen = false;
      this.load();
    });
  }
}
