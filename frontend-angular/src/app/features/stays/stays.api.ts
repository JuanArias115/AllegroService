import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClientService } from '../../core/api/http-client.service';
import { CheckOutResponse, CheckoutSummary, Consumption, Stay } from '../../core/models/domain.model';

@Injectable({ providedIn: 'root' })
export class StaysApi {
  constructor(private readonly http: HttpClientService) {}

  get(id: string): Observable<Stay> {
    return this.http.get<Stay>(`/v1/stays/${id}`);
  }

  getConsumptions(id: string): Observable<Consumption[]> {
    return this.http.get<Consumption[]>(`/v1/stays/${id}/consumptions`);
  }

  addConsumption(
    id: string,
    payload: {
      source: number;
      description: string;
      locationId?: string;
      allowOverridePrice: boolean;
      items: { productId?: string; qty: number; unitPrice?: number }[];
    }
  ): Observable<Consumption> {
    return this.http.post<Consumption>(`/v1/stays/${id}/consumptions`, payload);
  }

  getCheckoutSummary(id: string, lang?: string): Observable<CheckoutSummary> {
    return this.http.get<CheckoutSummary>(`/v1/stays/${id}/checkout-summary`, { lang });
  }

  checkOut(id: string, payload: { force: boolean; checkOutAt?: string }): Observable<CheckOutResponse> {
    return this.http.post<CheckOutResponse>(`/v1/stays/${id}/check-out`, payload);
  }
}
