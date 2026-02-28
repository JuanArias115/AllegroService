import { APP_INITIALIZER, ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { routes } from './app.routes';
import { RuntimeConfigService } from './core/config/runtime-config.service';
import { FirebaseAuthService } from './core/auth/firebase-auth.service';
import { authInterceptor } from './core/api/auth.interceptor';
import { errorInterceptor } from './core/api/error.interceptor';

function initializeApp(runtimeConfig: RuntimeConfigService, auth: FirebaseAuthService): () => Promise<void> {
  return async () => {
    await runtimeConfig.load();
    await auth.initialize();
    if (auth.isAuthenticated) {
      try {
        await auth.loadUserTenant();
      } catch {
        // Guard/no-access view will handle blocked users.
      }
    }
  };
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor, errorInterceptor])),
    {
      provide: APP_INITIALIZER,
      multi: true,
      deps: [RuntimeConfigService, FirebaseAuthService],
      useFactory: initializeApp
    }
  ]
};
