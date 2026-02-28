import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs/operators';
import { CategoriesApi } from './categories.api';
import { Category } from '../../core/models/domain.model';
import { ToastService } from '../../core/ui/toast.service';
import { DataTableComponent, TableColumn } from '../../shared/components/data-table/data-table.component';
import { ModalComponent } from '../../shared/components/modal/modal.component';
import { FormFieldComponent } from '../../shared/components/form-field/form-field.component';

@Component({
  selector: 'app-categories-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DataTableComponent, ModalComponent, FormFieldComponent],
  templateUrl: './categories-page.component.html'
})
export class CategoriesPageComponent implements OnInit {
  readonly columns: TableColumn[] = [{ key: 'name', label: 'Name' }];

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required]]
  });

  categories: Category[] = [];
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
    private readonly api: CategoriesApi,
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
        this.categories = response.items;
        this.rows = response.items.map((category) => ({ ...category }));
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
    this.form.reset({ name: '' });
    this.modalOpen = true;
  }

  openEdit(row: Record<string, unknown>): void {
    const id = String(row['id']);
    const found = this.categories.find((category) => category.id === id);
    if (!found) {
      return;
    }

    this.selectedId = found.id;
    this.form.reset({ name: found.name });
    this.modalOpen = true;
  }

  remove(row: Record<string, unknown>): void {
    const id = String(row['id']);
    if (!confirm('Delete this category?')) {
      return;
    }

    this.api.remove(id).subscribe(() => {
      this.toast.success('Category removed.');
      this.load();
    });
  }

  submit(): void {
    if (this.form.invalid || this.saving) {
      this.form.markAllAsTouched();
      return;
    }

    const payload = { name: this.form.controls.name.value };

    this.saving = true;
    const request$ = this.selectedId ? this.api.update(this.selectedId, payload) : this.api.create(payload);

    request$.pipe(finalize(() => (this.saving = false))).subscribe(() => {
      this.toast.success(this.selectedId ? 'Category updated.' : 'Category created.');
      this.modalOpen = false;
      this.load();
    });
  }
}
