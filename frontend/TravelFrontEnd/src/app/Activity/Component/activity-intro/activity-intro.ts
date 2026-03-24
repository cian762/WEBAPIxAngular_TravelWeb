import { reviewResponseDTO } from './../../Interface/reviewResonseDTO';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AfterViewInit, Component, ElementRef, inject, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivityInfoService } from '../../Service/activity-info-service';
import { ActivityInfoInterface } from '../../Interface/InfoInterface';
import { productInfoInterface } from '../../Interface/productIntroInterface';
import { reviewsPackage } from '../../Interface/reviewsPackage';
import { FormsModule } from '@angular/forms';
import { RouteService } from '../../Service/route-service';

import { NgFor, NgIf } from '@angular/common';
import { RouteOptionDto } from '../../Interface/routeOptionDto';
import { CardInfoModel } from '../../Interface/cardInterface';
import { TicketPlanDrawer } from '../ticket-plan-drawer/ticket-plan-drawer';
import { TicketInfoService } from '../../Service/ticket-info-service';
import { ticketInfoInterface } from '../../Interface/ticketInfoInterface';
import { forkJoin, Subscription, switchMap } from 'rxjs';
import { UserCommentForm } from "../user-comment-form/user-comment-form";
import { PersonalCommentService } from '../../Service/personal-comment-service';
import { EditCommentForm } from "../edit-comment-form/edit-comment-form";

@Component({
  selector: 'app-activity-intro',
  imports: [FormsModule, NgIf, NgFor, RouterLink, TicketPlanDrawer, UserCommentForm, EditCommentForm],
  templateUrl: './activity-intro.html',
  styleUrl: './activity-intro.css',
})
export class ActivityIntro implements OnInit, AfterViewInit, OnDestroy {

  activityIdFromRoute: number = 0;
  isMapViewReady = false;

  private sub?: Subscription;

  activityInfo: ActivityInfoInterface = {
    activityId: 0,
    title: '',
    types: [],
    description: '',
    startTime: new Date(),
    endTime: new Date(),
    address: '',
    longitude: 0,
    latitude: 0,
    propaganda: '',
    officialLink: '',
    images: [],
    regions: []
  };

  productInfoCollection: productInfoInterface[] = [{
    productCode: '',
    productName: '',
    currentPrice: 0,
    notes: '',
    coverImageUrl: '',
    status: ''
  }];

  reviewsPackage: reviewsPackage = {
    activityId: 0,
    reviews: [],
    averageRating: 0,
    commentCount: 0
  };


  suggestionCollection: CardInfoModel[] = [];

  personalCommentCollection: reviewResponseDTO[] = [];

  @ViewChild('mapContainer', { static: false }) mapContainer!: ElementRef<HTMLDivElement>;
  map!: google.maps.Map;

  private infoService = inject(ActivityInfoService);
  private activatedRoute = inject(ActivatedRoute);
  private routeService = inject(RouteService);
  private ticketInfoService = inject(TicketInfoService);
  private personalCommentService = inject(PersonalCommentService);


  ngOnDestroy(): void {
    // this.sub?.unsubscribe();
  }

  ngOnInit(): void {
    this.sub = this.activatedRoute.params.subscribe((params) => {
      const id = Number(params['id']);
      if (!id) return;

      this.activityIdFromRoute = id;

      this.suggestionCollection = [];
      this.productInfoCollection = [];
      this.reviewsPackage = {
        activityId: 0,
        reviews: [],
        averageRating: 0,
        commentCount: 0
      };

      this.routeOptions = [];
      this.selectedRouteIndex = 0;
      this.routeInfo = {
        distanceText: '',
        durationText: ''
      };

      if (this.routePolyline) {
        this.routePolyline.setMap(null);
      }


      forkJoin({
        activityInfo: this.infoService.getActivityDetails(id),
        reviewsPackage: this.infoService.getRelatedReviews(id, this.selectedSortRule),
        productInfoCollection: this.infoService.getRelatedTickets(id),
        //TODO 這邊到時要把 memberId = 2 拿掉
        personalComments: this.personalCommentService.getPersonalComments(id, "2")
      }).subscribe({
        next: ({ activityInfo, reviewsPackage, productInfoCollection, personalComments }) => {
          this.activityInfo = activityInfo;
          this.reviewsPackage = reviewsPackage;
          this.productInfoCollection = productInfoCollection;
          this.personalCommentCollection = personalComments;
          this.tryInitMap();
          this.getRelatedActivitySuggestion();
        },
        error: (err) => {
          console.log('資料載入失敗', err);
        }
      });
    });


  }

  getActivityInfo(activityId: number) {
    return this.infoService.getActivityDetails(activityId);
  }


  getRelatedReviews(activityId: number) {
    console.log(this.selectedSortRule);
    return this.infoService.getRelatedReviews(activityId, this.selectedSortRule);
  }

  getRelatedTickets(activityId: number) {
    return this.infoService.getRelatedTickets(activityId);
  }


  getPersonalComments(activityId: number, memberId: string) {
    //TODO 這邊到時要把 memberId 拿掉
    return this.personalCommentService.getPersonalComments(activityId, memberId);
  }

  FindBookMark(bookmark: string) {
    const element = document.getElementById(bookmark);
    const offset = 100;

    if (element) {
      const elementPosition = element.getBoundingClientRect().top + window.scrollY;
      const offsetPosition = elementPosition - offset;
      window.scrollTo({ top: offsetPosition, behavior: 'smooth' });
    }
  }

  ngAfterViewInit(): void {
    this.isMapViewReady = true;
    this.tryInitMap();
  }

  tryInitMap(): void {
    if (!this.isMapViewReady) return;
    if (!this.activityInfo.latitude || !this.activityInfo.longitude) return;

    this.initMap();
  }

  initMap(): void {
    const center = {
      lat: Number(this.activityInfo.latitude),
      lng: Number(this.activityInfo.longitude)
    };

    this.map = new google.maps.Map(this.mapContainer.nativeElement, {
      center,
      zoom: 15
    });

    new google.maps.Marker({
      position: center,
      map: this.map,
      title: this.activityInfo.title,
    });

    setTimeout(() => {
      google.maps.event.trigger(this.map, 'resize');
      this.map.setCenter(center);
    }, 100);
    console.log('地圖載入成功');
  }




  //取得使用者定位
  userMarker?: google.maps.Marker;
  userPosition?: { lat: number; lng: number };


  getCurrentLocation(): void {
    if (!navigator.geolocation) {
      alert('瀏覽器不支援定位功能');
      return;
    }

    navigator.geolocation.getCurrentPosition(
      (position) => {
        this.userPosition = {
          lat: position.coords.latitude,
          lng: position.coords.longitude
        };

        if (this.userMarker) {
          this.userMarker.setMap(null);
        }

        this.userMarker = new google.maps.Marker({
          position: this.userPosition,
          map: this.map,
          title: '我的位置'
        });

        const bounds = new google.maps.LatLngBounds();

        bounds.extend(this.userPosition);

        if (this.activityInfo) {
          bounds.extend({
            lat: this.activityInfo.latitude,
            lng: this.activityInfo.longitude
          });
        }

        this.map.fitBounds(bounds);
      },
      (error) => {
        console.error('定位失敗', error);
        alert('無法取得位置，請確認是否允許定位權限');
      },
      {
        enableHighAccuracy: true
      }
    );
  }

  //選擇交通方式
  routePolyline?: google.maps.Polyline;
  selectedTravelMode: 'DRIVE' | 'WALK' | 'BICYCLE' = 'DRIVE';
  routeOptions: RouteOptionDto[] = [];
  selectedRouteIndex = 0;

  routeInfo = {
    distanceText: '',
    durationText: ''
  };

  getRoute() {
    if (!this.userPosition) {
      alert('請先取得目前位置');
      return;
    }

    const destination = {
      lat: Number(this.activityInfo.latitude),
      lng: Number(this.activityInfo.longitude)
    }


    this.routeService.getRoute({
      originLat: this.userPosition.lat,
      originLng: this.userPosition.lng,
      destinationLat: destination.lat,
      destinationLng: destination.lng,
      travelMode: this.selectedTravelMode
    }).subscribe({
      next: (result) => {
        this.routeOptions = result.routes;
        this.selectedRouteIndex = 0;
        if (this.routeOptions.length > 0) {
          this.drawSelectedRoute(0);
        }

      },
      error: (err) => {
        console.error(err);
        alert('目前無法取得這個交通方式路線');
      }
    });
  }

  drawSelectedRoute(index: number): void {
    this.selectedRouteIndex = index;

    const selectedRoute = this.routeOptions[index];
    if (!selectedRoute) return;

    if (this.routePolyline) {
      this.routePolyline.setMap(null);
    }

    const decodedPath = google.maps.geometry.encoding.decodePath(selectedRoute.encodedPolyline);

    this.routePolyline = new google.maps.Polyline({
      path: decodedPath,
      geodesic: true,
      strokeOpacity: 1,
      strokeWeight: 5,
      strokeColor: '#2563eb',

    });

    this.routePolyline.setMap(this.map);
  }

  getRelatedActivitySuggestion() {
    if (this.activityInfo.types.length) {
      console.log('有打進suggestion方法');

      const param = new SuggestionInfo();
      param.activityId = this.activityInfo.activityId;
      param.activityType = this.activityInfo.types;
      this.infoService.offerRelatedOptions(param)?.subscribe((data) => {
        this.suggestionCollection = data;
        console.log('建議的活動資訊', this.suggestionCollection);
      });
    }
  }


  //用商品對應的 productCode 去打 API 拿詳細的商品資訊
  isPlanDrawerOpen: boolean = false;
  selectedPlan: ticketInfoInterface | null = null;
  productInfo: ticketInfoInterface | null = null;

  getTicketInfo(productCode: string) {
    this.ticketInfoService.getTicketInfoService(productCode).subscribe((data) => {
      if (data) {
        this.productInfo = data;
        this.productInfo.tag = ['即買即用', '電子票', '可取消'];
        this.openPlanDrawer(this.productInfo);
      }
      console.log("ACT-XXXX資料", this.productInfo);

    });
  }

  openPlanDrawer(plan: ticketInfoInterface) {
    this.selectedPlan = plan;
    this.isPlanDrawerOpen = true;
  }

  closePlanDrawer() {
    this.isPlanDrawerOpen = false;
  }


  //評論篩選規則
  sortRuleCollection = [
    { label: 'highest', value: '最高評分' },
    { label: 'lowest', value: '最低評分' },
    { label: 'newest', value: '最新評論' },
    { label: 'picFirst', value: '圖片優先' }

  ];

  selectedSortRule: string = 'highest';
  changeSortRule(sortRule: string) {
    this.selectedSortRule = sortRule;
    this.activatedRoute.params.subscribe((params) => {
      const id = Number(params['id']);
      this.infoService.getRelatedReviews(id, this.selectedSortRule)
        .subscribe((data) => {
          this.reviewsPackage = data;
        });
    });
  }

  isModalOpen = false;
  toggleCommentForm() {
    this.isModalOpen = !this.isModalOpen;
    console.log('按下後', this.isModalOpen);
  }

  openCommentForm(): void {
    this.isModalOpen = true;
  }

  closeCommentForm(): void {
    console.log('父元件 closeCommentForm()被觸發');
    this.isModalOpen = false;
  }

  refreshPersonalComment(): void {
    console.log('父元件 refreshPersonalComment()被觸發');
    this.personalCommentService.getPersonalComments(this.activityIdFromRoute, "2").pipe(
      switchMap((data) => {
        console.log("個人評論更新載入成功");
        this.personalCommentCollection = data;

        return this.infoService.getRelatedReviews(this.activityIdFromRoute, this.selectedSortRule);
      })
    ).subscribe({
      next: (res) => {
        console.log('評論重新載入成功');
        this.reviewsPackage = res;
      },
      error: (err) => {
        console.log('refreshPersonalComment 發生錯誤', err);
      }
    })
  }

  deletePersonalComment(activityId: number): void {
    this.personalCommentService.deletePresonalComment(activityId)
      .subscribe({
        next: (res) => {
          console.log('刪除成功', res);
          this.refreshPersonalComment();
        },
        error: (err) => {
          console.log('刪除失敗', err);
        }
      });
  }

  openEditForm = false;

  isEditFormOpen() {
    console.log('壓下 isEditFormOpen');
    this.openEditForm = !this.openEditForm;
    this.refreshPersonalComment();
  }

  selectedPersonalReview: reviewResponseDTO = {
    title: "",
    reviewId: 0,
    memberId: '',
    comment: '',
    rating: 0,
    createDate: new Date(),
    reviewImages: []
  };

  sendTargetToEditForm(target: reviewResponseDTO) {
    this.isEditFormOpen();
    this.selectedPersonalReview = target;
  }

}

export class SuggestionInfo {
  activityId: number = 0;
  activityType: string[] = [];
}
