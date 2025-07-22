import {bootstrapApplication} from '@angular/platform-browser';
import {App} from './app/app';
import {appConfig} from "./appConfig";
import 'zone.js'; // ✅ Necessario per il change detection

bootstrapApplication(App, appConfig)
  .catch((err) => console.error(err));

