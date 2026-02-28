import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClientService } from '../../core/api/http-client.service';
import { ListQuery, PagedResponse } from '../../core/models/api.model';
import { Location } from '../../core/models/domain.model';

export type LocationPayload = Pick<Location, 'name' | 'type' | 'unitId'>;

@Injectable({ providedIn: 'root' })
export class LocationsApi {
  constructor(private readonly http: HttpClientService) {}

  list(query: Partial<ListQuery>): Observable<PagedResponse<Location>> {
    return this.http.get<PagedResponse<Location>>('/v1/locations', query);
  }

  all(): Observable<PagedResponse<Location>> {
    return this.list({ page: 1, pageSize: 200, sort: 'name' });
  }

  create(payload: LocationPayload): Observable<Location> {
    return this.http.post<Location>('/v1/locations', payload);
  }

  update(id: string, payload: LocationPayload): Observable<Location> {
    return this.http.put<Location>(`/v1/locations/${id}`, payload);
  }

  remove(id: string): Observable<boolean> {
    return this.http.delete(`/v1/locations/${id}`);
  }
}
