import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs/operators';
import { CategoriesApi } from '../categories/categories.api';
import { Product } from '../../core/models/domain.model';
import { ToastService } from '../../core/ui/toast.service';
import { DataTableComponent, TableColumn } from '../../shared/components/data-table/data-table.component';
import { ModalComponent } from '../../shared/components/modal/modal.component';
import { FormFieldComponent } from '../../shared/components/form-field/form-field.component';
import { ProductsApi } from './products.api';

@Component({
  selector: 'app-products-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, DataTableComponent, ModalComponent, FormFieldComponent],
  templateUrl: './products-page.component.html'
})
export class ProductsPageComponent implements OnInit {
  readonly columns: TableColumn[] = [
    { key: 'sku', label: 'SKU' },
    { key: 'name', label: 'Name' },
    { key: 'categoryName', label: 'Category' },
    { key: 'salePrice', label: 'Sale price' },
    { key: 'trackStockLabel', label: 'Track stock' }
  ];

  readonly form = this.fb.nonNullable.group({
    sku: ['', [Validators.required]],
    name: ['', [Validators.required]],
    categoryId: ['', [Validators.required]],
    unit: ['', [Validators.required]],
    salePrice: [0, [Validators.required, Validators.min(0)]],
    costPrice: [0],
    isActive: [true],
    trackStock: [true]
  });

  categories: { id: string; name: string }[] = [];
  products: Product[] = [];
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
    private readonly api: ProductsApi,
    private readonly categoriesApi: CategoriesApi,
    private readonly toast: ToastService
  ) {}

  ngOnInit(): void {
    this.loadCategories();
    this.load();
  }

  loadCategories(): void {
    this.categoriesApi.all().subscribe((response) => {
      this.categories = response.items;
    });
  }

  load(): void {
    this.loading = true;
    this.api
      .list({ page: this.page, pageSize: this.pageSize, search: this.search, sort: 'name' })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe((response) => {
        this.products = response.items;
        this.rows = response.items.map((product) => ({
          ...product,
          salePrice: product.salePrice.toFixed(2),
          trackStockLabel: product.trackStock ? 'Yes' : 'No'
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
    this.form.reset({
      sku: '',
      name: '',
      categoryId: '',
      unit: '',
      salePrice: 0,
      costPrice: 0,
      isActive: true,
      trackStock: true
    });
    this.modalOpen = true;
  }

  openEdit(row: Record<string, unknown>): void {
    const id = String(row['id']);
    const found = this.products.find((product) => product.id === id);
    if (!found) {
      return;
    }

    this.selectedId = found.id;
    this.form.reset({
      sku: found.sku,
      name: found.name,
      categoryId: found.categoryId,
      unit: found.unit,
      salePrice: found.salePrice,
      costPrice: found.costPrice ?? 0,
      isActive: found.isActive,
      trackStock: found.trackStock
    });
    this.modalOpen = true;
  }

  remove(row: Record<string, unknown>): void {
    const id = String(row['id']);
    if (!confirm('Delete this product?')) {
      return;
    }

    this.api.remove(id).subscribe(() => {
      this.toast.success('Product removed.');
      this.load();
    });
  }

  submit(): void {
    if (this.form.invalid || this.saving) {
      this.form.markAllAsTouched();
      return;
    }

    const payload = {
      sku: this.form.controls.sku.value,
      name: this.form.controls.name.value,
      categoryId: this.form.controls.categoryId.value,
      unit: this.form.controls.unit.value,
      salePrice: Number(this.form.controls.salePrice.value),
      costPrice: Number(this.form.controls.costPrice.value),
      isActive: this.form.controls.isActive.value,
      trackStock: this.form.controls.trackStock.value
    };

    this.saving = true;
    const request$ = this.selectedId ? this.api.update(this.selectedId, payload) : this.api.create(payload);

    request$.pipe(finalize(() => (this.saving = false))).subscribe(() => {
      this.toast.success(this.selectedId ? 'Product updated.' : 'Product created.');
      this.modalOpen = false;
      this.load();
    });
  }
}
