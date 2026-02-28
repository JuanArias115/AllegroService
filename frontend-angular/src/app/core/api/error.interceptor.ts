import { HttpContextToken, HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, from, switchMap, throwError } from 'rxjs';
import { FirebaseAuthService } from '../auth/firebase-auth.service';
import { ToastService } from '../ui/toast.service';

const RETRIED_401 = new HttpContextToken<boolean>(() => false);

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const auth = inject(FirebaseAuthService);
  const toast = inject(ToastService);

  const navigateToLogin = () => {
    void auth.signOut();
    void router.navigate(['/login']);
  };

  return next(req).pipe(
    catchError((error: unknown) => {
      if (!(error instanceof HttpErrorResponse)) {
        return throwError(() => error);
      }

      if (error.status === 401) {
        if (req.context.get(RETRIED_401)) {
          navigateToLogin();
          return throwError(() => error);
        }

        return from(auth.refreshIdToken()).pipe(
          switchMap((token) => {
            if (!token) {
              navigateToLogin();
              return throwError(() => error);
            }

            const retriedRequest = req.clone({
              context: req.context.set(RETRIED_401, true),
              setHeaders: {
                Authorization: `Bearer ${token}`
              }
            });

            return next(retriedRequest).pipe(
              catchError((retryError: unknown) => {
                if (retryError instanceof HttpErrorResponse && retryError.status === 401) {
                  navigateToLogin();
                }

                return throwError(() => retryError);
              })
            );
          }),
          catchError((refreshError: unknown) => {
            navigateToLogin();
            return throwError(() => refreshError);
          })
        );
      }

      if (error.status === 403) {
        auth.setNoAccessFromError(error);
        void router.navigate(['/no-access']);
      } else if (error.status >= 400) {
        toast.error(extractErrorMessage(error));
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
