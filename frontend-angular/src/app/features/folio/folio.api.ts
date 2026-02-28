import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Charge, FolioDetail, Payment } from '../../core/models/domain.model';
import { HttpClientService } from '../../core/api/http-client.service';

@Injectable({ providedIn: 'root' })
export class FolioApi {
  constructor(private readonly http: HttpClientService) {}

  get(folioId: string): Observable<FolioDetail> {
    return this.http.get<FolioDetail>(`/v1/folios/${folioId}`);
  }

  addCharge(
    folioId: string,
    payload: {
      source: number;
      description: string;
      locationId?: string;
      allowOverridePrice: boolean;
      items: { productId?: string; qty: number; unitPrice?: number }[];
      qty?: number;
      unitPrice?: number;
    }
  ): Observable<Charge> {
    return this.http.post<Charge>(`/v1/folios/${folioId}/charges`, payload);
  }

  addPayment(
    folioId: string,
    payload: { amount: number; method: number; reference?: string; paidAt?: string }
  ): Observable<Payment> {
    return this.http.post<Payment>(`/v1/folios/${folioId}/payments`, payload);
  }
}
