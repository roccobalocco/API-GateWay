import {ApplicationConfig, provideZonelessChangeDetection} from "@angular/core";
import {provideAnimationsAsync} from "@angular/platform-browser/animations/async";
import {providePrimeNG} from "primeng/config";
import Aura from "@primeuix/themes/aura";
import {provideHttpClient} from '@angular/common/http';
import {JWT_OPTIONS} from '@auth0/angular-jwt';


export function jwtOptionsFactory() {
  return {
    allowedDomains: ['localhost:4200'],
    disallowedRoutes: []
  };
}

export const appConfig: ApplicationConfig = {
  providers: [
    { provide: JWT_OPTIONS, useFactory: jwtOptionsFactory },
    provideHttpClient(),
    provideZonelessChangeDetection(),
    provideAnimationsAsync(),
    providePrimeNG({
      theme: {
        preset: Aura
      }
    })
  ]
};
