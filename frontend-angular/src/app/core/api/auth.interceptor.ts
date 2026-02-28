import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { from } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { FirebaseAuthService } from '../auth/firebase-auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(FirebaseAuthService);

  return from(auth.getValidToken()).pipe(
    switchMap((token) => {
      if (!token) {
        return next(req);
      }

      const withAuth = req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });

      return next(withAuth);
    })
  );
};
