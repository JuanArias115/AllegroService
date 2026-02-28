import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { FolioApi } from '../folio/folio.api';
import { LocationsApi } from '../locations/locations.api';
import { ProductsApi } from '../products/products.api';
import { StaysApi } from './stays.api';
import { CHARGE_SOURCE_OPTIONS, PAYMENT_METHOD_OPTIONS, labelOf } from '../../core/models/enums';
import { FolioDetail, Location, Product, Stay } from '../../core/models/domain.model';
import { ToastService } from '../../core/ui/toast.service';
import { ModalComponent } from '../../shared/components/modal/modal.component';
import { FormFieldComponent } from '../../shared/components/form-field/form-field.component';

@Component({
  selector: 'app-stay-detail',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, ModalComponent, FormFieldComponent],
  templateUrl: './stay-detail.component.html'
})
export class StayDetailComponent implements OnInit {
  readonly chargeSourceOptions = CHARGE_SOURCE_OPTIONS;
  readonly paymentMethodOptions = PAYMENT_METHOD_OPTIONS;

  readonly chargeForm = this.fb.nonNullable.group({
    source: [2, [Validators.required]],
    description: ['', [Validators.required]],
    locationId: [''],
    allowOverridePrice: [false],
    items: this.fb.array([this.createChargeItemGroup()])
  });

  readonly paymentForm = this.fb.nonNullable.group({
    amount: [0, [Validators.required, Validators.min(0.01)]],
    method: [1, [Validators.required]],
    reference: ['']
  });

  readonly checkOutForm = this.fb.nonNullable.group({
    force: [false]
  });

  stay: Stay | null = null;
  folio: FolioDetail | null = null;
  folioId: string | null = null;

  locations: Location[] = [];
  products: Product[] = [];

  loading = false;
  actionLoading = false;

  chargeModalOpen = false;
  paymentModalOpen = false;
  checkOutModalOpen = false;

  constructor(
    private readonly fb: FormBuilder,
    private readonly route: ActivatedRoute,
    private readonly staysApi: StaysApi,
    private readonly folioApi: FolioApi,
    private readonly locationsApi: LocationsApi,
    private readonly productsApi: ProductsApi,
    private readonly toast: ToastService
  ) {}

  ngOnInit(): void {
    this.loadLookups();
    this.load();
  }

  get chargeItems(): FormArray {
    return this.chargeForm.controls.items;
  }

  statusLabel(status: number): string {
    return status === 1 ? 'Open' : 'Closed';
  }

  paymentMethodLabel(method: number): string {
    return labelOf(this.paymentMethodOptions, method);
  }

  chargeSourceLabel(source: number): string {
    return labelOf(this.chargeSourceOptions, source);
  }

  createChargeItemGroup() {
    return this.fb.nonNullable.group({
      productId: [''],
      qty: [1, [Validators.required, Validators.min(0.001)]],
      unitPrice: [0]
    });
  }

  addChargeItem(): void {
    this.chargeItems.push(this.createChargeItemGroup());
  }

  removeChargeItem(index: number): void {
    if (this.chargeItems.length <= 1) {
      return;
    }

    this.chargeItems.removeAt(index);
  }

  loadLookups(): void {
    this.locationsApi.all().subscribe((response) => (this.locations = response.items));
    this.productsApi.all().subscribe((response) => (this.products = response.items));
  }

  load(): void {
    const stayId = this.route.snapshot.paramMap.get('id');
    if (!stayId) {
      return;
    }

    this.loading = true;
    this.staysApi
      .get(stayId)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe((stay) => {
        this.stay = stay;

        const queryFolioId = this.route.snapshot.queryParamMap.get('folioId');
        this.folioId = queryFolioId || stay.openFolioId || null;

        if (this.folioId) {
          this.loadFolio(this.folioId);
        }
      });
  }

  loadFolio(folioId: string): void {
    this.folioApi.get(folioId).subscribe((folio) => {
      this.folio = folio;
    });
  }

  openChargeModal(): void {
    this.chargeForm.reset({
      source: 2,
      description: '',
      locationId: '',
      allowOverridePrice: false,
      items: []
    });

    this.chargeItems.clear();
    this.chargeItems.push(this.createChargeItemGroup());
    this.chargeModalOpen = true;
  }

  submitCharge(): void {
    if (!this.folioId || this.chargeForm.invalid || this.actionLoading) {
      this.chargeForm.markAllAsTouched();
      return;
    }

    const items = this.chargeItems.controls.map((group) => ({
      productId: group.get('productId')?.value || undefined,
      qty: Number(group.get('qty')?.value ?? 0),
      unitPrice: Number(group.get('unitPrice')?.value ?? 0) || undefined
    }));

    const payload = {
      source: Number(this.chargeForm.controls.source.value),
      description: this.chargeForm.controls.description.value,
      locationId: this.chargeForm.controls.locationId.value || undefined,
      allowOverridePrice: this.chargeForm.controls.allowOverridePrice.value,
      items
    };

    this.actionLoading = true;
    this.folioApi
      .addCharge(this.folioId, payload)
      .pipe(finalize(() => (this.actionLoading = false)))
      .subscribe(() => {
        this.toast.success('Charge added.');
        this.chargeModalOpen = false;
        this.loadFolio(this.folioId!);
      });
  }

  openPaymentModal(): void {
    this.paymentForm.reset({ amount: 0, method: 1, reference: '' });
    this.paymentModalOpen = true;
  }

  submitPayment(): void {
    if (!this.folioId || this.paymentForm.invalid || this.actionLoading) {
      this.paymentForm.markAllAsTouched();
      return;
    }

    const payload = {
      amount: Number(this.paymentForm.controls.amount.value),
      method: Number(this.paymentForm.controls.method.value),
      reference: this.paymentForm.controls.reference.value || undefined
    };

    this.actionLoading = true;
    this.folioApi
      .addPayment(this.folioId, payload)
      .pipe(finalize(() => (this.actionLoading = false)))
      .subscribe(() => {
        this.toast.success('Payment added.');
        this.paymentModalOpen = false;
        this.loadFolio(this.folioId!);
      });
  }

  openCheckOutModal(): void {
    this.checkOutForm.reset({ force: false });
    this.checkOutModalOpen = true;
  }

  submitCheckOut(): void {
    if (!this.stay || this.actionLoading) {
      return;
    }

    const payload = {
      force: this.checkOutForm.controls.force.value
    };

    this.actionLoading = true;
    this.staysApi
      .checkOut(this.stay.id, payload)
      .pipe(finalize(() => (this.actionLoading = false)))
      .subscribe(() => {
        this.toast.success('Check-out completed.');
        this.checkOutModalOpen = false;
        this.load();
      });
  }
}
