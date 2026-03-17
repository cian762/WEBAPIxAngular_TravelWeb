import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'itinerary',
    loadComponent: () => import('./Itinerary/component/index-itinerary/index-itinerary').then(m => m.IndexItinerary),
    children: [{
      path: 'change',
      loadComponent: () => import('./Itinerary/component/change-itinerary-item/change-itinerary-item').then(m => m.ChangeItineraryItem)
    }]
  }
];
