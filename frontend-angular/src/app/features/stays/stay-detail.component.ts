import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { finalize } from 'rxjs/operators';
import { FolioApi } from '../folio/folio.api';
import { LocationsApi } from '../locations/locations.api';
import { ProductsApi } from '../products/products.api';
import { StaysApi } from './stays.api';
import { FolioDetail, Location, Product, Stay, Consumption, CheckoutSummary } from '../../core/models/domain.model';
import { ToastService } from '../../core/ui/toast.service';
import { ModalComponent } from '../../shared/components/modal/modal.component';
import { FormFieldComponent } from '../../shared/components/form-field/form-field.component';
import { IconComponent } from '../../shared/components/icon/icon.component';
import { TooltipComponent } from '../../shared/components/tooltip/tooltip.component';
import { I18nService } from '../../core/i18n/i18n.service';

@Component({
  selector: 'app-stay-detail',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, ModalComponent, FormFieldComponent, IconComponent, TooltipComponent, TranslateModule],
  templateUrl: './stay-detail.component.html'
})
export class StayDetailComponent implements OnInit {
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
    force: [false],
    sendWhatsapp: [false],
    phone: ['']
  });

  stay: Stay | null = null;
  folio: FolioDetail | null = null;
  folioId: string | null = null;
  consumptions: Consumption[] = [];
  checkoutSummary: CheckoutSummary | null = null;
  whatsAppUrl: string | null = null;

  locations: Location[] = [];
  products: Product[] = [];

  loading = false;
  actionLoading = false;
  summaryLoading = false;

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
    private readonly toast: ToastService,
    private readonly translate: TranslateService,
    private readonly i18n: I18nService
  ) {}

  ngOnInit(): void {
    this.loadLookups();
    this.load();
    this.checkOutForm.controls.phone.valueChanges.subscribe(() => this.updateWhatsAppUrl());
    this.checkOutForm.controls.sendWhatsapp.valueChanges.subscribe(() => this.updateWhatsAppUrl());
  }

  get chargeItems(): FormArray {
    return this.chargeForm.controls.items;
  }

  get consumptionsTotal(): number {
    return this.consumptions.reduce((acc, item) => acc + item.total, 0);
  }

  get canOpenWhatsapp(): boolean {
    return !!this.whatsAppUrl && !!this.checkOutForm.controls.sendWhatsapp.value;
  }

  get currentPhone(): string {
    return this.checkOutForm.controls.phone.value?.trim() || this.checkoutSummary?.phone || '';
  }

  statusLabel(status: number): string {
    return status === 1 ? this.translate.instant('folio.open') : this.translate.instant('folio.closed');
  }

  stayStatusLabel(status: number): string {
    switch (status) {
      case 1:
        return this.translate.instant('stayStatus.checkedIn');
      case 2:
        return this.translate.instant('stayStatus.checkedOut');
      case 3:
        return this.translate.instant('stayStatus.cancelled');
      default:
        return String(status);
    }
  }

  paymentMethodLabel(method: number): string {
    switch (method) {
      case 1:
        return this.translate.instant('paymentMethod.cash');
      case 2:
        return this.translate.instant('paymentMethod.card');
      case 3:
        return this.translate.instant('paymentMethod.transfer');
      case 4:
        return this.translate.instant('paymentMethod.online');
      default:
        return String(method);
    }
  }

  chargeSourceLabel(source: number): string {
    switch (source) {
      case 1:
        return this.translate.instant('chargeSource.room');
      case 2:
        return this.translate.instant('chargeSource.minibar');
      case 3:
        return this.translate.instant('chargeSource.restaurant');
      case 4:
        return this.translate.instant('chargeSource.extra');
      default:
        return String(source);
    }
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

        this.loadConsumptions();
      });
  }

  loadFolio(folioId: string): void {
    this.folioApi.get(folioId).subscribe((folio) => {
      this.folio = folio;
    });
  }

  loadConsumptions(): void {
    if (!this.stay) {
      return;
    }

    this.staysApi.getConsumptions(this.stay.id).subscribe((consumptions) => {
      this.consumptions = consumptions;
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
    if (!this.stay || this.chargeForm.invalid || this.actionLoading) {
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
    this.staysApi
      .addConsumption(this.stay.id, payload)
      .pipe(finalize(() => (this.actionLoading = false)))
      .subscribe(() => {
        this.toast.success(this.translate.instant('messages.consumptionSaved'));
        this.chargeModalOpen = false;
        this.loadConsumptions();
        if (this.folioId) {
          this.loadFolio(this.folioId);
        }
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
        this.toast.success(this.translate.instant('messages.paymentSaved'));
        this.paymentModalOpen = false;
        this.loadFolio(this.folioId!);
      });
  }

  openCheckOutModal(): void {
    this.checkOutForm.reset({ force: false, sendWhatsapp: false, phone: this.checkoutSummary?.phone ?? '' });
    this.checkOutModalOpen = true;
    this.refreshCheckoutSummary();
  }

  refreshCheckoutSummary(): void {
    if (!this.stay) {
      return;
    }

    this.summaryLoading = true;
    this.staysApi
      .getCheckoutSummary(this.stay.id, this.i18n.current)
      .pipe(finalize(() => (this.summaryLoading = false)))
      .subscribe((summary) => {
        this.checkoutSummary = summary;
        if (!this.checkOutForm.controls.phone.value) {
          this.checkOutForm.controls.phone.setValue(summary.phone ?? '', { emitEvent: false });
        }
        this.updateWhatsAppUrl();
      });
  }

  openWhatsapp(): void {
    if (this.whatsAppUrl) {
      window.open(this.whatsAppUrl, '_blank', 'noopener,noreferrer');
    }
  }

  submitCheckOut(): void {
    if (!this.stay || this.actionLoading) {
      return;
    }

    const openWhatsappAfterClose = this.canOpenWhatsapp;
    const whatsappUrl = this.whatsAppUrl;
    const payload = {
      force: this.checkOutForm.controls.force.value
    };

    this.actionLoading = true;
    this.staysApi
      .checkOut(this.stay.id, payload)
      .pipe(finalize(() => (this.actionLoading = false)))
      .subscribe(() => {
        this.toast.success(this.translate.instant('messages.checkoutCompleted'));
        this.checkOutModalOpen = false;
        this.load();
        if (openWhatsappAfterClose && whatsappUrl) {
          window.open(whatsappUrl, '_blank', 'noopener,noreferrer');
        }
      });
  }

  private updateWhatsAppUrl(): void {
    if (!this.checkOutForm.controls.sendWhatsapp.value || !this.checkoutSummary?.message) {
      this.whatsAppUrl = null;
      return;
    }

    const normalizedPhone = this.normalizePhone(this.currentPhone);
    if (!normalizedPhone) {
      this.whatsAppUrl = null;
      return;
    }

    this.whatsAppUrl = `https://wa.me/${normalizedPhone}?text=${encodeURIComponent(this.checkoutSummary.message)}`;
  }

  private normalizePhone(raw: string | null | undefined): string | null {
    if (!raw) {
      return null;
    }

    const cleaned = raw.replace(/[^\d+]/g, '');
    if (!cleaned) {
      return null;
    }

    if (cleaned.startsWith('+')) {
      const digits = cleaned.slice(1).replace(/\D/g, '');
      return digits.length >= 8 ? digits : null;
    }

    const digits = cleaned.replace(/\D/g, '');
    return digits.length >= 8 ? digits : null;
  }
}
