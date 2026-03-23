import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideZoneChangeDetection } from '@angular/core';
import { provideRouter, withInMemoryScrolling } from '@angular/router';
// import { provideHttpClient, withFetch } from '@angular/common/http';
import { routes } from './app.routes';
import { provideHttpClient } from '@angular/common/http';





export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes,
      withInMemoryScrolling({ scrollPositionRestoration: 'top' }//防止網頁記憶造成進入時滑動
      )),
    provideZoneChangeDetection(),
    provideHttpClient(),
    // provideHttpClient(withFetch()),

  ]
};
