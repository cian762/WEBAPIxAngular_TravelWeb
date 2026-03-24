import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideZoneChangeDetection } from '@angular/core';
import { provideRouter, withInMemoryScrolling } from '@angular/router';
// import { provideHttpClient, withFetch } from '@angular/common/http';
import { routes } from './app.routes';
// 引入攔截器提供者
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
// 引入我們剛剛寫的攔截器
import { authInterceptor } from './Member/interceptors/auth-interceptor';





export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes, withInMemoryScrolling({
      scrollPositionRestoration: 'top',
      anchorScrolling: 'disabled'
    })),
    provideHttpClient(
      withFetch(),
      withInterceptors([authInterceptor])
    ),
    provideZoneChangeDetection(),
    provideHttpClient()

  ]
};
