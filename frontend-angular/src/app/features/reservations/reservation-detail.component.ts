import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { Reservation } from '../../core/models/domain.model';
import { ToastService } from '../../core/ui/toast.service';
import { ModalComponent } from '../../shared/components/modal/modal.component';
import { FormFieldComponent } from '../../shared/components/form-field/form-field.component';
import { RESERVATION_STATUS_OPTIONS, labelOf } from '../../core/models/enums';
import { ReservationsApi } from './reservations.api';

@Component({
  selector: 'app-reservation-detail',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, ModalComponent, FormFieldComponent],
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

  reservation: Reservation | null = null;
  loading = false;
  saving = false;
  checkInModalOpen = false;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly fb: FormBuilder,
    private readonly api: ReservationsApi,
    private readonly toast: ToastService
  ) {}

  ngOnInit(): void {
    this.load();
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
      });
  }

  statusLabel(status: number): string {
    return labelOf(this.statusOptions, status);
  }

  openCheckInModal(): void {
    this.checkInForm.reset({ checkInAt: '', roomUnitPrice: 0, roomNights: 1, roomDescription: 'Room charge' });
    this.checkInModalOpen = true;
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
        this.toast.success('Check-in completed.');
        this.checkInModalOpen = false;
        this.router.navigate(['/stays', response.stayId], { queryParams: { folioId: response.folioId } });
      });
  }
}
