import { Routes } from '@angular/router';
import { TripProductDetail } from './trip/component/trip-product-detail/trip-product-detail';
import { aGuard } from './a-guard';
import { ProfileComponent } from './Member/profile/profile.component';
import { ResetPasswordComponent } from './Member/reset-password/reset-password.component';

export const routes: Routes = [
  {
    path: 'ActivityInfo',
    children: [
      {
        path: '',
        loadComponent: () => import('./Activity/Component/info-card/info-card')
          .then(m => m.InfoCard)
      },
      {
        path: ':id',
        loadComponent: () => import('./Activity/Component/activity-intro/activity-intro')
          .then(m => m.ActivityIntro)
      }

    ]
  },

  // 景點介紹開始
  {
    path: 'attractions',  // ← 改這裡
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
      {
        path: 'tags',
        loadComponent: () =>
          import('./Components/attractions/attraction-tags/attraction-tags')
            .then(m => m.AttractionTagsComponent),
      }
    ],
  },
  // 景點介紹結束
  //行程商品相關
  { path: 'trip-detail/:id', component: TripProductDetail },
  {
    path: 'tripProduct',
    loadComponent: () => import('./trip/component/product/product').then(
      m => m.Product
    )
  }, {
    path: 'Shoppingcart',
    loadComponent: () => import('./trip/component/shoppingcart/shoppingcart').then(
      m => m.Shoppingcart
    )
  },
  {
    path: 'order',
    loadComponent: () => import('./trip/component/order/order').then(
      m => m.Order
    )
    , canActivate: [aGuard]
  },
  /**行程建立路由*/
  {
    path: 'itinerary',
    loadComponent: () => import('./Itinerary/component/index-itinerary/index-itinerary').then(m => m.IndexItinerary)
  },
  {
    path: 'itinerary-detail/:id',
    loadComponent: () => import('./Itinerary/component/change-itinerary-item/change-itinerary-item').then(m => m.ItineraryDetailComponent),
    canActivate: [aGuard]
  }, {
    path: 'Itinerarylist',
    loadComponent: () => import('./Itinerary/component/itinerarylist/itinerarylist').then(m => m.Itinerarylist),
    canActivate: [aGuard]
  },

  {
    path: 'Board',
    children: [
      {
        path: '',
        loadComponent: () => import('./Board/blog-home/blog-home').then(m => m.BlogHome),
      },
      //創建快速文章
      {
        path: 'creat/:id',
        loadComponent: () => import('./Board/creat-post/creat-post').then(m => m.CreatPost),
      },
      //創建部落格
      {
        path: 'creatBlog',
        loadComponent: () => import('./Board/creat-blog/creat-blog').then(m => m.CreatBlog),
      },
      {
        path: 'detail/:id',
        loadComponent: () => import('./Board/post-detail/post-detail').then(m => m.PostDetail),
      },
      //私人(可編輯)
      {
        path: 'Main',
        loadComponent: () => import('./Board/personal-homepage/personal-homepage').then(m => m.PersonalHomepage),
      },
      //公開(只能看)
      {
        path: 'user/:id',
        loadComponent: () => import('./Board/user-articles-page/user-articles-page').then(m => m.UserArticlesPage),
      },
    ],
  },

  {
    path: '',
    loadComponent: () =>
      import('./travelindex/travelindex').then(m => m.Travelindex),
  },

  {
    path: 'login',
    loadComponent: () => import('./Member/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./Member/register/register.component').then(m => m.RegisterComponent)
  },
   { path: 'reset-password', component: ResetPasswordComponent },
  {
    path: 'profile',
    component: ProfileComponent, // 這是你目前顯示頭像和 Sidebar 的元件
    children: [
      {
        path: 'orderdetails', // 這裡不要加 /
        loadComponent: () => import('./trip/component/order-details/order-details').then(
          m => m.OrderDetails
        )
      }],
  },

  { path: 'complaint', loadComponent: () => import('./Member/complaint-form/complaint-form.component').then(m => m.ComplaintFormComponent) },

  // 所有不認識的路徑會導向首頁
  { path: '**', redirectTo: '' },
];
