import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { finalize } from 'rxjs/operators';
import { Consumption, Location, Product, Reservation } from '../../core/models/domain.model';
import { ToastService } from '../../core/ui/toast.service';
import { ModalComponent } from '../../shared/components/modal/modal.component';
import { FormFieldComponent } from '../../shared/components/form-field/form-field.component';
import { RESERVATION_STATUS_OPTIONS, labelOf } from '../../core/models/enums';
import { ReservationsApi } from './reservations.api';
import { ProductsApi } from '../products/products.api';
import { LocationsApi } from '../locations/locations.api';
import { IconComponent } from '../../shared/components/icon/icon.component';
import { TooltipComponent } from '../../shared/components/tooltip/tooltip.component';

@Component({
  selector: 'app-reservation-detail',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, ModalComponent, FormFieldComponent, TranslateModule, IconComponent, TooltipComponent],
  templateUrl: './reservation-detail.component.html'
})
export class ReservationDetailComponent implements OnInit {
  readonly statusOptions = RESERVATION_STATUS_OPTIONS;

  readonly checkInForm = this.fb.nonNullable.group({
    checkInAt: [''],
    roomUnitPrice: [0, [Validators.min(0)]],
    roomNights: [1, [Validators.min(1)]],
    roomDescription: ['Room charge']
  });

  readonly consumptionForm = this.fb.nonNullable.group({
    source: [2, [Validators.required]],
    description: ['', [Validators.required]],
    locationId: [''],
    allowOverridePrice: [false],
    items: this.fb.array([this.createConsumptionItemGroup()])
  });

  reservation: Reservation | null = null;
  consumptions: Consumption[] = [];
  products: Product[] = [];
  locations: Location[] = [];
  loading = false;
  saving = false;
  checkInModalOpen = false;
  consumptionModalOpen = false;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly fb: FormBuilder,
    private readonly api: ReservationsApi,
    private readonly productsApi: ProductsApi,
    private readonly locationsApi: LocationsApi,
    private readonly toast: ToastService,
    private readonly translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.loadLookups();
    this.load();
  }

  get consumptionItems(): FormArray {
    return this.consumptionForm.controls.items;
  }

  get consumptionTotal(): number {
    return this.consumptions.reduce((acc, item) => acc + item.total, 0);
  }

  load(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      return;
    }

    this.loading = true;
    this.api
      .get(id)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe((reservation) => {
        this.reservation = reservation;
        this.loadConsumptions();
      });
  }

  loadLookups(): void {
    this.productsApi.all().subscribe((response) => (this.products = response.items));
    this.locationsApi.all().subscribe((response) => (this.locations = response.items));
  }

  loadConsumptions(): void {
    if (!this.reservation) {
      return;
    }

    this.api.getConsumptions(this.reservation.id).subscribe((items) => {
      this.consumptions = items;
    });
  }

  statusLabel(status: number): string {
    const raw = labelOf(this.statusOptions, status);
    return this.translate.instant(`reservation.${raw.toLowerCase()}`);
  }

  sourceLabel(source: number): string {
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

  createConsumptionItemGroup() {
    return this.fb.nonNullable.group({
      productId: [''],
      qty: [1, [Validators.required, Validators.min(0.001)]],
      unitPrice: [0]
    });
  }

  addConsumptionItem(): void {
    this.consumptionItems.push(this.createConsumptionItemGroup());
  }

  removeConsumptionItem(index: number): void {
    if (this.consumptionItems.length <= 1) {
      return;
    }

    this.consumptionItems.removeAt(index);
  }

  openCheckInModal(): void {
    this.checkInForm.reset({ checkInAt: '', roomUnitPrice: 0, roomNights: 1, roomDescription: 'Room charge' });
    this.checkInModalOpen = true;
  }

  openConsumptionModal(): void {
    this.consumptionForm.reset({
      source: 2,
      description: '',
      locationId: '',
      allowOverridePrice: false,
      items: []
    });
    this.consumptionItems.clear();
    this.consumptionItems.push(this.createConsumptionItemGroup());
    this.consumptionModalOpen = true;
  }

  executeCheckIn(): void {
    if (!this.reservation || this.saving) {
      return;
    }

    const payload = {
      checkInAt: this.checkInForm.controls.checkInAt.value || undefined,
      roomUnitPrice: this.checkInForm.controls.roomUnitPrice.value > 0 ? Number(this.checkInForm.controls.roomUnitPrice.value) : undefined,
      roomNights: this.checkInForm.controls.roomNights.value > 0 ? Number(this.checkInForm.controls.roomNights.value) : undefined,
      roomDescription: this.checkInForm.controls.roomDescription.value || undefined
    };

    this.saving = true;
    this.api
      .checkIn(this.reservation.id, payload)
      .pipe(finalize(() => (this.saving = false)))
      .subscribe((response) => {
        this.toast.success(this.translate.instant('messages.checkinCompleted'));
        this.checkInModalOpen = false;
        this.router.navigate(['/stays', response.stayId], { queryParams: { folioId: response.folioId } });
      });
  }

  submitConsumption(): void {
    if (!this.reservation || this.saving || this.consumptionForm.invalid) {
      this.consumptionForm.markAllAsTouched();
      return;
    }

    const payload = {
      source: Number(this.consumptionForm.controls.source.value),
      description: this.consumptionForm.controls.description.value,
      locationId: this.consumptionForm.controls.locationId.value || undefined,
      allowOverridePrice: this.consumptionForm.controls.allowOverridePrice.value,
      items: this.consumptionItems.controls.map((group) => ({
        productId: group.get('productId')?.value || undefined,
        qty: Number(group.get('qty')?.value ?? 0),
        unitPrice: Number(group.get('unitPrice')?.value ?? 0) || undefined
      }))
    };

    this.saving = true;
    this.api
      .addConsumption(this.reservation.id, payload)
      .pipe(finalize(() => (this.saving = false)))
      .subscribe(() => {
        this.toast.success(this.translate.instant('messages.consumptionSaved'));
        this.consumptionModalOpen = false;
        this.loadConsumptions();
      });
  }
}
