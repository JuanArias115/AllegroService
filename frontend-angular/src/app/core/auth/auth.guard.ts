import { CanActivateFn, Router, UrlTree } from '@angular/router';
import { inject } from '@angular/core';
import { FirebaseAuthService } from './firebase-auth.service';

export const authGuard: CanActivateFn = async (): Promise<boolean | UrlTree> => {
  const auth = inject(FirebaseAuthService);
  const router = inject(Router);

  await auth.waitUntilInitialized();

  if (!auth.isAuthenticated) {
    return router.parseUrl('/login');
  }

  try {
    await auth.loadUserTenant();
  } catch {
    return router.parseUrl('/no-access');
  }

  if (!auth.isActiveSession()) {
    return router.parseUrl('/no-access');
  }

  return true;
};

export const loginGuard: CanActivateFn = async (): Promise<boolean | UrlTree> => {
  const auth = inject(FirebaseAuthService);
  const router = inject(Router);

  await auth.waitUntilInitialized();

  if (auth.isAuthenticated) {
    try {
      await auth.loadUserTenant();
      if (auth.isActiveSession()) {
        return router.parseUrl('/');
      }

      return router.parseUrl('/no-access');
    } catch {
      return router.parseUrl('/no-access');
    }
  }

  return true;
};
