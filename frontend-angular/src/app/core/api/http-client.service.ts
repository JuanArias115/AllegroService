import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { ApiEnvelope, ListQuery } from '../models/api.model';
import { RuntimeConfigService } from '../config/runtime-config.service';

@Injectable({ providedIn: 'root' })
export class HttpClientService {
  constructor(
    private readonly http: HttpClient,
    private readonly runtimeConfig: RuntimeConfigService
  ) {}

  get<T>(path: string, query?: Partial<ListQuery> | Record<string, string | number | boolean | null | undefined>): Observable<T> {
    return this.http
      .get<ApiEnvelope<T>>(this.resolve(path), { params: this.buildParams(query) })
      .pipe(map((response) => response.data));
  }

  post<T>(path: string, body: unknown): Observable<T> {
    return this.http
      .post<ApiEnvelope<T>>(this.resolve(path), body)
      .pipe(map((response) => response.data));
  }

  put<T>(path: string, body: unknown): Observable<T> {
    return this.http
      .put<ApiEnvelope<T>>(this.resolve(path), body)
      .pipe(map((response) => response.data));
  }

  delete(path: string): Observable<boolean> {
    return this.http
      .delete<ApiEnvelope<boolean>>(this.resolve(path))
      .pipe(map((response) => response.data));
  }

  private resolve(path: string): string {
    const base = this.runtimeConfig.value.apiBaseUrl.replace(/\/$/, '');
    const cleanPath = path.startsWith('/') ? path : `/${path}`;
    return `${base}${cleanPath}`;
  }

  private buildParams(
    query?: Partial<ListQuery> | Record<string, string | number | boolean | null | undefined>
  ): HttpParams {
    let params = new HttpParams();

    if (!query) {
      return params;
    }

    Object.entries(query).forEach(([key, value]) => {
      if (value !== null && value !== undefined && value !== '') {
        params = params.set(key, String(value));
      }
    });

    return params;
  }
}
