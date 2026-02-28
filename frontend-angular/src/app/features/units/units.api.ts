import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClientService } from '../../core/api/http-client.service';
import { ListQuery, PagedResponse } from '../../core/models/api.model';
import { Unit } from '../../core/models/domain.model';

export type UnitPayload = Pick<Unit, 'name' | 'type' | 'capacity' | 'status'>;

@Injectable({ providedIn: 'root' })
export class UnitsApi {
  constructor(private readonly http: HttpClientService) {}

  list(query: Partial<ListQuery>): Observable<PagedResponse<Unit>> {
    return this.http.get<PagedResponse<Unit>>('/v1/units', query);
  }

  get(id: string): Observable<Unit> {
    return this.http.get<Unit>(`/v1/units/${id}`);
  }

  create(payload: Omit<UnitPayload, 'status'>): Observable<Unit> {
    return this.http.post<Unit>('/v1/units', payload);
  }

  update(id: string, payload: UnitPayload): Observable<Unit> {
    return this.http.put<Unit>(`/v1/units/${id}`, payload);
  }

  remove(id: string): Observable<boolean> {
    return this.http.delete(`/v1/units/${id}`);
  }
}
