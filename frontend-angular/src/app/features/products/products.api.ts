import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClientService } from '../../core/api/http-client.service';
import { ListQuery, PagedResponse } from '../../core/models/api.model';
import { Product } from '../../core/models/domain.model';

export type ProductPayload = Pick<
  Product,
  'sku' | 'name' | 'categoryId' | 'unit' | 'salePrice' | 'costPrice' | 'isActive' | 'trackStock'
>;

@Injectable({ providedIn: 'root' })
export class ProductsApi {
  constructor(private readonly http: HttpClientService) {}

  list(query: Partial<ListQuery>): Observable<PagedResponse<Product>> {
    return this.http.get<PagedResponse<Product>>('/v1/products', query);
  }

  all(): Observable<PagedResponse<Product>> {
    return this.list({ page: 1, pageSize: 200, sort: 'name' });
  }

  create(payload: ProductPayload): Observable<Product> {
    return this.http.post<Product>('/v1/products', payload);
  }

  update(id: string, payload: ProductPayload): Observable<Product> {
    return this.http.put<Product>(`/v1/products/${id}`, payload);
  }

  remove(id: string): Observable<boolean> {
    return this.http.delete(`/v1/products/${id}`);
  }
}
