import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClientService } from '../../core/api/http-client.service';
import { ListQuery, PagedResponse } from '../../core/models/api.model';
import { Category } from '../../core/models/domain.model';

@Injectable({ providedIn: 'root' })
export class CategoriesApi {
  constructor(private readonly http: HttpClientService) {}

  list(query: Partial<ListQuery>): Observable<PagedResponse<Category>> {
    return this.http.get<PagedResponse<Category>>('/v1/categories', query);
  }

  all(): Observable<PagedResponse<Category>> {
    return this.list({ page: 1, pageSize: 200, sort: 'name' });
  }

  create(payload: Pick<Category, 'name'>): Observable<Category> {
    return this.http.post<Category>('/v1/categories', payload);
  }

  update(id: string, payload: Pick<Category, 'name'>): Observable<Category> {
    return this.http.put<Category>(`/v1/categories/${id}`, payload);
  }

  remove(id: string): Observable<boolean> {
    return this.http.delete(`/v1/categories/${id}`);
  }
}
