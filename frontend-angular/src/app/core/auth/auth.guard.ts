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

  if (!auth.hasGlampingAccess) {
    return router.parseUrl('/no-access');
  }

  return true;
};

export const loginGuard: CanActivateFn = async (): Promise<boolean | UrlTree> => {
  const auth = inject(FirebaseAuthService);
  const router = inject(Router);

  await auth.waitUntilInitialized();

  if (auth.isAuthenticated && auth.hasGlampingAccess) {
    return router.parseUrl('/');
  }

  return true;
};
