export interface ApiError {
  code: string;
  message: string;
}

export interface ApiEnvelope<T> {
  data: T;
  errors: ApiError[];
}

export interface PagedResponse<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
}

export interface ListQuery {
  page: number;
  pageSize: number;
  search?: string;
  sort?: string;
}
