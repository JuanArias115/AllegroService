import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { GuestsApi } from '../guests/guests.api';
import { UnitsApi } from '../units/units.api';
import { Guest, Reservation, Unit } from '../../core/models/domain.model';
import { ToastService } from '../../core/ui/toast.service';
import { ModalComponent } from '../../shared/components/modal/modal.component';
import { FormFieldComponent } from '../../shared/components/form-field/form-field.component';
import { RESERVATION_STATUS_OPTIONS, labelOf } from '../../core/models/enums';
import { ReservationsApi } from './reservations.api';

@Component({
  selector: 'app-reservations-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, ModalComponent, FormFieldComponent],
  templateUrl: './reservations-page.component.html'
})
export class ReservationsPageComponent implements OnInit {
  readonly statusOptions = RESERVATION_STATUS_OPTIONS;

  readonly form = this.fb.nonNullable.group({
    code: ['', [Validators.required]],
    guestId: ['', [Validators.required]],
    unitId: [''],
    checkInDate: ['', [Validators.required]],
    checkOutDate: ['', [Validators.required]],
    totalEstimated: [0, [Validators.required, Validators.min(0)]],
    status: [1, [Validators.required]]
  });

  guests: Guest[] = [];
  units: Unit[] = [];

  reservations: Reservation[] = [];
  filteredReservations: Reservation[] = [];

  selectedId: string | null = null;
  loading = false;
  saving = false;
  modalOpen = false;

  page = 1;
  pageSize = 10;
  total = 0;
  search = '';
  selectedStatus = 0;

  constructor(
    private readonly fb: FormBuilder,
    private readonly api: ReservationsApi,
    private readonly guestsApi: GuestsApi,
    private readonly unitsApi: UnitsApi,
    private readonly toast: ToastService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.loadDependencies();
    this.load();
  }

  loadDependencies(): void {
    this.guestsApi.list({ page: 1, pageSize: 200, sort: 'fullName' }).subscribe((response) => {
      this.guests = response.items;
    });

    this.unitsApi.list({ page: 1, pageSize: 200, sort: 'name' }).subscribe((response) => {
      this.units = response.items;
    });
  }

  load(): void {
    this.loading = true;
    this.api
      .list({ page: this.page, pageSize: this.pageSize, search: this.search, sort: 'checkin_desc' })
      .pipe(finalize(() => (this.loading = false)))
      .subscribe((response) => {
        this.reservations = response.items;
        this.total = response.total;
        this.applyFilter();
      });
  }

  applyFilter(): void {
    this.filteredReservations = this.selectedStatus
      ? this.reservations.filter((reservation) => reservation.status === this.selectedStatus)
      : [...this.reservations];
  }

  onSearch(value: string): void {
    this.search = value;
    this.page = 1;
    this.load();
  }

  onStatusChange(value: string): void {
    this.selectedStatus = Number(value);
    this.applyFilter();
  }

  onPageChange(step: number): void {
    const nextPage = this.page + step;
    if (nextPage < 1) {
      return;
    }

    this.page = nextPage;
    this.load();
  }

  statusLabel(status: number): string {
    return labelOf(this.statusOptions, status);
  }

  guestLabel(guestId: string): string {
    return this.guests.find((guest) => guest.id === guestId)?.fullName ?? guestId;
  }

  unitLabel(unitId?: string | null): string {
    if (!unitId) {
      return '-';
    }

    return this.units.find((unit) => unit.id === unitId)?.name ?? unitId;
  }

  openCreate(): void {
    this.selectedId = null;
    this.form.reset({
      code: '',
      guestId: '',
      unitId: '',
      checkInDate: '',
      checkOutDate: '',
      totalEstimated: 0,
      status: 1
    });
    this.modalOpen = true;
  }

  openEdit(reservation: Reservation): void {
    this.selectedId = reservation.id;
    this.form.reset({
      code: reservation.code,
      guestId: reservation.guestId,
      unitId: reservation.unitId ?? '',
      checkInDate: reservation.checkInDate,
      checkOutDate: reservation.checkOutDate,
      totalEstimated: reservation.totalEstimated,
      status: reservation.status
    });
    this.modalOpen = true;
  }

  viewDetail(reservation: Reservation): void {
    this.router.navigate(['/reservations', reservation.id]);
  }

  remove(reservation: Reservation): void {
    if (!confirm('Delete this reservation?')) {
      return;
    }

    this.api.remove(reservation.id).subscribe(() => {
      this.toast.success('Reservation removed.');
      this.load();
    });
  }

  submit(): void {
    if (this.form.invalid || this.saving) {
      this.form.markAllAsTouched();
      return;
    }

    const payload = {
      code: this.form.controls.code.value,
      guestId: this.form.controls.guestId.value,
      unitId: this.form.controls.unitId.value || null,
      checkInDate: this.form.controls.checkInDate.value,
      checkOutDate: this.form.controls.checkOutDate.value,
      totalEstimated: Number(this.form.controls.totalEstimated.value),
      status: Number(this.form.controls.status.value)
    };

    this.saving = true;

    const request$ = this.selectedId
      ? this.api.update(this.selectedId, {
          guestId: payload.guestId,
          unitId: payload.unitId,
          checkInDate: payload.checkInDate,
          checkOutDate: payload.checkOutDate,
          totalEstimated: payload.totalEstimated,
          status: payload.status
        })
      : this.api.create(payload);

    request$.pipe(finalize(() => (this.saving = false))).subscribe(() => {
      this.toast.success(this.selectedId ? 'Reservation updated.' : 'Reservation created.');
      this.modalOpen = false;
      this.load();
    });
  }
}
