import { bootstrapApplication } from '@angular/platform-browser';
import { App } from './app/app.component';   // <-- corrected path
import { provideHttpClient } from '@angular/common/http';

bootstrapApplication(App, {
  providers: [provideHttpClient()],
});
