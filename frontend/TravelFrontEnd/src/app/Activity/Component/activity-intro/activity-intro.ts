import { ActivatedRoute, RouterLink } from '@angular/router';
import { AfterViewInit, Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { ActivityInfoService } from '../../Service/activity-info-service';
import { ActivityInfoInterface } from '../../Interface/InfoInterface';
import { productInfoInterface } from '../../Interface/productIntroInterface';
import { reviewsPackage } from '../../Interface/reviewsPackage';
import { FormsModule } from '@angular/forms';
import { RouteService } from '../../Service/route-service';

import { NgFor, NgIf } from '@angular/common';
import { RouteOptionDto } from '../../Interface/routeOptionDto';
import { CardInfoModel } from '../../Interface/cardInterface';

@Component({
  selector: 'app-activity-intro',
  imports: [FormsModule, NgIf, NgFor, RouterLink],
  templateUrl: './activity-intro.html',
  styleUrl: './activity-intro.css',
})
export class ActivityIntro implements OnInit, AfterViewInit {

  activityIdFromRoute: number = 0;
  isMapViewReady = false;

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
    productCode: 0,
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

  @ViewChild('mapContainer', { static: false }) mapContainer!: ElementRef<HTMLDivElement>;
  map!: google.maps.Map;

  constructor(
    private infoService: ActivityInfoService,
    private activatedRoute: ActivatedRoute,
    private routeService: RouteService) { }

  ngOnInit(): void {
    this.activatedRoute.params.subscribe((params) => {
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

      this.getActivityInfo(id);
      this.getRelatedReviews(id);
      this.getRelatedTickets(id);

      this.routeOptions = [];
      this.selectedRouteIndex = 0;
      this.routeInfo = {
        distanceText: '',
        durationText: ''
      };

      if (this.routePolyline) {
        this.routePolyline.setMap(null);
      }
    });
  }

  getActivityInfo(activityId: number) {
    this.infoService.getActivityDetails(activityId)?.subscribe((data) => {
      this.activityInfo = data;
      console.log('這是activityinfo', this.activityInfo);
      this.tryInitMap();
      this.getRelatedActivitySuggestion();
    });
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


  getRelatedReviews(activityId: number) {
    this.infoService.getRelatedReviews(activityId)?.subscribe((data) => {
      this.reviewsPackage = data;
      console.log('這是reviewPackage', this.reviewsPackage);
    });
  }

  getRelatedTickets(activityId: number) {
    this.infoService.getRelatedTickets(activityId)?.subscribe((data) => {
      this.productInfoCollection = data;
      console.log('這是proinfocol', this.productInfoCollection);
    });
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
}

export class SuggestionInfo {
  activityId: number = 0;
  activityType: string[] = [];
}
