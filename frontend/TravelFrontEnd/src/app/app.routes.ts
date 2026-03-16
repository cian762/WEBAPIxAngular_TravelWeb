import { InfoCard } from './Activity/Component/info-card/info-card';
import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'ActivityInfo',
    loadComponent: () => import('./Activity/Component/info-card/info-card').then(m => m.InfoCard)
  }
];
