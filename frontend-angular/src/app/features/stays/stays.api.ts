import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClientService } from '../../core/api/http-client.service';
import { CheckOutResponse, Stay } from '../../core/models/domain.model';

@Injectable({ providedIn: 'root' })
export class StaysApi {
  constructor(private readonly http: HttpClientService) {}

  get(id: string): Observable<Stay> {
    return this.http.get<Stay>(`/v1/stays/${id}`);
  }

  checkOut(id: string, payload: { force: boolean; checkOutAt?: string }): Observable<CheckOutResponse> {
    return this.http.post<CheckOutResponse>(`/v1/stays/${id}/check-out`, payload);
  }
}
