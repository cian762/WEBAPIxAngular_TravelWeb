import { AuthService } from './../../../Member/services/auth.service';
import { reviewResponseDTO } from './../../Interface/reviewResonseDTO';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AfterViewInit, Component, ElementRef, HostListener, inject, OnDestroy, OnInit, ViewChild } from '@angular/core';
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
import { EMPTY, finalize, forkJoin, of, Subscription, switchMap, tap } from 'rxjs';
import { UserCommentForm } from "../user-comment-form/user-comment-form";
import { PersonalCommentService } from '../../Service/personal-comment-service';
import { EditCommentForm } from "../edit-comment-form/edit-comment-form";
import Swal from 'sweetalert2';

@Component({
  selector: 'app-activity-intro',
  imports: [FormsModule, NgIf, NgFor, RouterLink, TicketPlanDrawer, UserCommentForm, EditCommentForm],
  templateUrl: './activity-intro.html',
  styleUrl: './activity-intro.css',
})

export class ActivityIntro implements OnInit, AfterViewInit {

  activityIdFromRoute: number = 0;
  isMapViewReady = false;
  isPageLoading = true;
  isSuggestionLoading = true;

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
    regions: [],
    commentCount: 0,
    sellCount: 0,
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
  private authService = inject(AuthService);
  private router = inject(Router);


  ngOnInit(): void {
    this.updateCardsPerView();
    this.activatedRoute.params.pipe(
      tap(() => {
        this.isPageLoading = true;
        this.isSuggestionLoading = true;

        this.suggestionCollection = [];
        this.productInfoCollection = [];
        this.personalCommentCollection = [];
        this.currentIndex = 0;

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
      }),
      switchMap(params => {
        const id = Number(params['id']);
        if (!id) {
          this.isPageLoading = false;
          this.isSuggestionLoading = false;
          return EMPTY;
        }

        this.activityIdFromRoute = id;

        return forkJoin({
          activityInfo: this.infoService.getActivityDetails(id),
          reviewsPackage: this.infoService.getRelatedReviews(id, this.selectedSortRule),
          productInfoCollection: this.infoService.getRelatedTickets(id),
          personalComments: this.personalCommentService.getPersonalComments(id)
        });
      })
    ).subscribe({
      next: ({ activityInfo, reviewsPackage, productInfoCollection, personalComments }) => {
        this.activityInfo = activityInfo;
        this.reviewsPackage = reviewsPackage;
        this.productInfoCollection = productInfoCollection;
        this.personalCommentCollection = personalComments;

        // 先把正式畫面渲染出來
        this.isPageLoading = false;

        // 推薦活動先跑，避免被地圖初始化錯誤卡住
        this.getRelatedActivitySuggestion();

        // 等 DOM 把 #mapContainer 渲染出來後再初始化地圖
        setTimeout(() => {
          this.tryInitMap();
        }, 0);
      },
      error: (err) => {
        this.isPageLoading = false;
        this.isSuggestionLoading = false;
        console.log('資料載入失敗', err);
      }
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


  getPersonalComments(activityId: number) {
    return this.personalCommentService.getPersonalComments(activityId);
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





  //Google Map 使用到的方法
  tryInitMap(): void {
    if (!this.isMapViewReady) return;
    if (!this.mapContainer?.nativeElement) return;
    if (!this.activityInfo.latitude || !this.activityInfo.longitude) return;

    this.initMap();
  }

  initMap(): void {
    if (!this.mapContainer?.nativeElement) return;

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
      ('瀏覽器不支援定位功能');
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
        Swal.fire({
          icon: "error",
          title: "無法取得位置",
          text: "請確認是否允許定位權限",
        });
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
      Swal.fire({
        icon: "info",
        title: "請先取得當前位置",
      });
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
        Swal.fire({
          icon: "error",
          title: "無法取得該交通資訊",
          text: "目前無法取得這個交通方式路線",
        });
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





  //拿相關的活動推薦用方法
  getRelatedActivitySuggestion() {
    this.isSuggestionLoading = true;
    this.currentIndex = 0;

    if (!this.activityInfo.types.length) {
      this.suggestionCollection = [];
      this.isSuggestionLoading = false;
      return;
    }

    console.log('有打進suggestion方法');

    const param = new SuggestionInfo();
    param.activityId = this.activityInfo.activityId;
    param.activityType = this.activityInfo.types;

    this.infoService.offerRelatedOptions(param)?.pipe(
      finalize(() => {
        this.isSuggestionLoading = false;
      })
    ).subscribe({
      next: (data) => {
        this.suggestionCollection = data ?? [];
        console.log('建議的活動資訊', this.suggestionCollection);
      },
      error: (err) => {
        console.log('推薦活動載入失敗', err);
        this.suggestionCollection = [];
      }
    });
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


  //預設的評論篩選條件為 'highest'
  selectedSortRule: string = 'highest';
  changeSortRule(sortRule: string) {
    this.selectedSortRule = sortRule;
    this.infoService.getRelatedReviews(this.activityIdFromRoute, this.selectedSortRule)
      .subscribe((data) => {
        console.log(data);
        this.reviewsPackage = data;
      });
  }



  //用來觸動 子Component (user-Comment-Form)
  isModalOpen = false;

  toggleCommentForm() {
    this.authService.checkAuthStatus().subscribe({
      next: (res) => {
        if (res === false) {
          Swal.fire({
            icon: "warning",
            title: "請確認登入狀態",
            timer: 1000,
            showConfirmButton: false,
          });
          this.router.navigate(['login'], { queryParams: { returnUrl: this.router.url } });
          return;
        }
        console.log('res', res);
        this.isModalOpen = !this.isModalOpen;
        console.log('按下後', this.isModalOpen);
      },
      error: (err) => {
        console.error('檢查登入狀態', err);
        this.router.navigate(['login']);
      }
    });
  }

  openCommentForm(): void {
    this.isModalOpen = true;
  }

  closeCommentForm(): void {
    console.log('父元件 closeCommentForm()被觸發');
    this.isModalOpen = false;
  }



  //當關閉 子Component (user-Comment-Form) 後要觸發的事件
  refreshPersonalComment(): void {
    console.log('父元件 refreshPersonalComment()被觸發');
    this.personalCommentService.getPersonalComments(this.activityIdFromRoute).pipe(
      switchMap((data) => {
        console.log("個人評論更新載入成功");
        this.personalCommentCollection = data;

        //觸動整體評論更新，是為了做到評分也跟著動態修改
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


  //個人刪除評論使用的方法
  deletePersonalComment(activityId: number): void {
    Swal.fire({
      title: "是否要刪除此評論?",
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#3085d6",
      cancelButtonColor: "#d33",
      confirmButtonText: "確認刪除",
      cancelButtonText: "取消"
    }).then((result) => {
      if (!result.isConfirmed) return;

      this.personalCommentService.deletePersonalComment(activityId)
        .subscribe({
          next: (res) => {
            Swal.fire({
              title: "已刪除評論",
              icon: "success"
            });

            this.refreshPersonalComment();
            console.log('刪除成功', res);
          },
          error: (err) => {
            Swal.fire({
              title: "刪除失敗",
              text: "請稍後再試",
              icon: "error"
            });

            console.log('刪除失敗', err);
          }
        });
    });
  }


  //個人評論編輯相關的 子Component (Edit-Comment-Form) 觸動變數/方法
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
    reviewImages: [],
    memberAvatar: '',
  };

  sendTargetToEditForm(target: reviewResponseDTO) {
    this.isEditFormOpen();
    this.selectedPersonalReview = target;
  }


  //推薦分頁邏輯
  cardsPerView = 3;
  cardWidthPercent = 100 / this.cardsPerView;
  currentIndex = 0;

  @HostListener('window:resize')
  onResize(): void {
    this.updateCardsPerView();
  }

  private updateCardsPerView(): void {
    const width = window.innerWidth;

    if (width < 768) {
      this.cardsPerView = 1;
    } else if (width < 992) {
      this.cardsPerView = 2;
    } else {
      this.cardsPerView = 3;
    }

    this.cardWidthPercent = 100 / this.cardsPerView;

    const maxIndex = Math.max(0, this.suggestionCollection.length - this.cardsPerView);
    if (this.currentIndex > maxIndex) {
      this.currentIndex = maxIndex;
    }
  }

  next(): void {
    const maxIndex = Math.max(0, this.suggestionCollection.length - this.cardsPerView);
    if (this.currentIndex < maxIndex) {
      this.currentIndex++;
    }
  }

  prev(): void {
    if (this.currentIndex > 0) {
      this.currentIndex--;
    }
  }


  email: string = "";
  comment: string = "";

  onSubmit() {

    console.log(this.email);
    console.log(this.comment);

    if (this.email === "" && this.comment === "") {
      Swal.fire({
        position: "center",
        icon: "warning",
        title: "請完整填入資訊",
        showConfirmButton: false,
        timer: 1500
      });
      return;
    }

    if (!this.email.includes("@")) {
      Swal.fire({
        position: "center",
        icon: "warning",
        title: "請填入正確Email格式",
        showConfirmButton: false,
        timer: 1500
      });
      return;
    }

    Swal.fire({
      position: "center",
      icon: "success",
      title: "感謝您提供的問題回報",
      showConfirmButton: false,
      timer: 1500
    });

    this.email = "";
    this.comment = "";
  }

  keyInData() {
    this.email = "abc123@gmail.com";
    this.comment = "活動日期已經有更新，請管理員協助修改";
  }


}




export class SuggestionInfo {
  activityId: number = 0;
  activityType: string[] = [];
}
