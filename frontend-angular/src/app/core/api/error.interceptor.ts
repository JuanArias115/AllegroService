import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { FirebaseAuthService } from '../auth/firebase-auth.service';
import { ToastService } from '../ui/toast.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const auth = inject(FirebaseAuthService);
  const toast = inject(ToastService);

  return next(req).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse) {
        const message = extractErrorMessage(error);

        if (error.status === 401) {
          auth.logout();
          router.navigate(['/login']);
        } else if (error.status === 403) {
          router.navigate(['/no-access']);
        } else if (error.status >= 400) {
          toast.error(message);
        }
      }

      return throwError(() => error);
    })
  );
};

function extractErrorMessage(error: HttpErrorResponse): string {
  const payload = error.error;

  if (typeof payload === 'string' && payload.trim().length > 0) {
    return payload;
  }

  if (payload?.detail) {
    return payload.detail;
  }

  if (Array.isArray(payload?.errors) && payload.errors[0]?.message) {
    return payload.errors[0].message;
  }

  return `Request failed (${error.status}).`;
}
