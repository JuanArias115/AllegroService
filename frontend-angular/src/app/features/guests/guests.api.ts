import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClientService } from '../../core/api/http-client.service';
import { ListQuery, PagedResponse } from '../../core/models/api.model';
import { Guest } from '../../core/models/domain.model';

export type GuestPayload = Pick<Guest, 'fullName' | 'documentId' | 'phone' | 'email'>;

@Injectable({ providedIn: 'root' })
export class GuestsApi {
  constructor(private readonly http: HttpClientService) {}

  list(query: Partial<ListQuery>): Observable<PagedResponse<Guest>> {
    return this.http.get<PagedResponse<Guest>>('/v1/guests', query);
  }

  get(id: string): Observable<Guest> {
    return this.http.get<Guest>(`/v1/guests/${id}`);
  }

  create(payload: GuestPayload): Observable<Guest> {
    return this.http.post<Guest>('/v1/guests', payload);
  }

  update(id: string, payload: GuestPayload): Observable<Guest> {
    return this.http.put<Guest>(`/v1/guests/${id}`, payload);
  }

  remove(id: string): Observable<boolean> {
    return this.http.delete(`/v1/guests/${id}`);
  }
}
