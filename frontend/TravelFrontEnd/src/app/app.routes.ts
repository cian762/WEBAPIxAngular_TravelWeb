import { InfoCard } from './Activity/Component/info-card/info-card';
import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'ActivityInfo',
    loadComponent: () => import('./Activity/Component/info-card/info-card').then(m => m.InfoCard)
  },
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
  {
    path: 'itinerary',
    loadComponent: () => import('./Itinerary/component/index-itinerary/index-itinerary').then(m => m.IndexItinerary),
    children: [{
      path: 'change',
      loadComponent: () => import('./Itinerary/component/change-itinerary-item/change-itinerary-item').then(m => m.ChangeItineraryItem)
    }]
  },
  {
    path: 'Board',
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./Board/blog-home/blog-home')
            .then(m => m.BlogHome),
      },
      {
        path: 'detail/:id',
        loadComponent: () =>
          import('./Board/post-detail/post-detail')
            .then(m => m.PostDetail),
      },
      {
        path: 'creat/:id',
        loadComponent: () =>
          import('./Board/creat-post/creat-post')
            .then(m => m.CreatPost),
      },

    ],
  },
  // 所有不認識的路徑會導向首頁
  { path: '**', redirectTo: '' },


];
