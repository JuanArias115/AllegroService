import { APP_INITIALIZER, ApplicationConfig, importProvidersFrom } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { TranslateLoader, TranslateModule } from '@ngx-translate/core';
import { routes } from './app.routes';
import { RuntimeConfigService } from './core/config/runtime-config.service';
import { FirebaseAuthService } from './core/auth/firebase-auth.service';
import { authInterceptor } from './core/api/auth.interceptor';
import { errorInterceptor } from './core/api/error.interceptor';
import { HttpTranslateLoader } from './core/i18n/http-translate.loader';
import { I18nService } from './core/i18n/i18n.service';
import { ThemeService } from './core/theme/theme.service';

function createTranslateLoader(http: HttpClient): TranslateLoader {
  return new HttpTranslateLoader(http);
}

function initializeApp(
  runtimeConfig: RuntimeConfigService,
  auth: FirebaseAuthService,
  i18n: I18nService,
  theme: ThemeService
): () => Promise<void> {
  return async () => {
    await runtimeConfig.load();
    theme.initialize();
    i18n.initialize();
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
    importProvidersFrom(
      TranslateModule.forRoot({
        loader: {
          provide: TranslateLoader,
          useFactory: createTranslateLoader,
          deps: [HttpClient]
        }
      })
    ),
    {
      provide: APP_INITIALIZER,
      multi: true,
      deps: [RuntimeConfigService, FirebaseAuthService, I18nService, ThemeService],
      useFactory: initializeApp
    }
  ]
};
