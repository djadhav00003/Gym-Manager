// src/main.ts
import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { provideRouter } from '@angular/router';
import { routes } from './app/app.routes';
import { importProvidersFrom } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { provideFirebaseApp, initializeApp } from '@angular/fire/app';
import { provideStorage, getStorage } from '@angular/fire/storage';
import { environment } from './environments/environment';

// Interceptors
import { AuthInterceptor } from './app/interceptors/auth.interceptor';
import { WithCredentialsInterceptor } from './app/interceptors/with-credentials.interceptor';

// Helper: get runtime firebase config from window.appConfig, fallback to environment
function getRuntimeFirebaseConfig() {
  try {
    const cfg = (window as any).appConfig?.firebaseConfig;
    if (cfg && Object.keys(cfg).length) {
      console.log('Using runtime Firebase config from app-config.json');
      return cfg;
    }
  } catch (e) {
    // ignore
  }
  console.log('Using fallback Firebase config from environment.ts');
  return environment.firebaseConfig;
}

bootstrapApplication(AppComponent, {
  providers: [
    // ensure HttpClient is registered (classic, robust approach)
    importProvidersFrom(HttpClientModule, FormsModule),

    // Register interceptors (class-based) via HTTP_INTERCEPTORS
    {
      provide: HTTP_INTERCEPTORS,
      useClass: WithCredentialsInterceptor,
      multi: true
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true
    },

    // router + firebase + animations
    provideRouter(routes),
   // Initialize Firebase using runtime config (window.appConfig) with fallback
    provideFirebaseApp(() => initializeApp(getRuntimeFirebaseConfig() as any)),

    provideStorage(() => getStorage()),
    provideAnimationsAsync()
  ],
}).catch(err => console.error(err));
