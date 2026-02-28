import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClientService } from '../../core/api/http-client.service';
import { ListQuery, PagedResponse } from '../../core/models/api.model';
import { CheckInResponse, Reservation } from '../../core/models/domain.model';

export type ReservationPayload = Pick<
  Reservation,
  'code' | 'guestId' | 'unitId' | 'checkInDate' | 'checkOutDate' | 'status' | 'totalEstimated'
>;

@Injectable({ providedIn: 'root' })
export class ReservationsApi {
  constructor(private readonly http: HttpClientService) {}

  list(query: Partial<ListQuery> & { status?: number }): Observable<PagedResponse<Reservation>> {
    return this.http.get<PagedResponse<Reservation>>('/v1/reservations', query);
  }

  get(id: string): Observable<Reservation> {
    return this.http.get<Reservation>(`/v1/reservations/${id}`);
  }

  create(payload: ReservationPayload): Observable<Reservation> {
    return this.http.post<Reservation>('/v1/reservations', payload);
  }

  update(id: string, payload: Omit<ReservationPayload, 'code'>): Observable<Reservation> {
    return this.http.put<Reservation>(`/v1/reservations/${id}`, payload);
  }

  remove(id: string): Observable<boolean> {
    return this.http.delete(`/v1/reservations/${id}`);
  }

  checkIn(id: string, payload: { checkInAt?: string; roomUnitPrice?: number; roomNights?: number; roomDescription?: string }): Observable<CheckInResponse> {
    return this.http.post<CheckInResponse>(`/v1/reservations/${id}/check-in`, payload);
  }
}
