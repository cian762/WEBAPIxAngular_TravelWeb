import { InfoCard } from './Activity/Component/info-card/info-card';
import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'ActivityInfo',
    loadComponent: () => import('./Activity/Component/info-card/info-card').then(m => m.InfoCard)
{
    path: '',
    loadComponent: () =>
      import('./Components/test-use/test-use').then(m => m.TestUse),
  },

  // 景點介紹開始
  {
    path: 'contact',
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./Components/attractions/attractions-home/attractions-home')
            .then(m => m.AttractionsHomeComponent),
      },
      {
        path: 'list',
        loadComponent: () =>
          import('./Components/attractions/attraction-list/attraction-list')
            .then(m => m.AttractionListComponent),
      },
      {
        path: 'detail/:id',
        loadComponent: () =>
          import('./Components/attractions/attraction-detail/attraction-detail')
            .then(m => m.AttractionDetailComponent),
      },
    ],
  },
 // 景點介紹結束







// 所有不認識的路徑會導向首頁
  { path: '**', redirectTo: '' },



  {
    path: 'itinerary',
    loadComponent: () => import('./Itinerary/component/index-itinerary/index-itinerary').then(m => m.IndexItinerary),
    children: [{
      path: 'change',
      loadComponent: () => import('./Itinerary/component/change-itinerary-item/change-itinerary-item').then(m => m.ChangeItineraryItem)
    }]
  }
];
