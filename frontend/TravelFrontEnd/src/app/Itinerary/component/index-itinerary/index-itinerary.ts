import { AfterViewInit, Component, ElementRef, Input, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Mainservice } from '../../service/mainservice';
import { Router } from '@angular/router';

declare var google: any;
@Component({
  selector: 'app-index-itinerary',
  imports: [FormsModule],
  templateUrl: './index-itinerary.html',
  styleUrl: './index-itinerary.css',
})
export class IndexItinerary implements AfterViewInit {

  constructor(private mainService: Mainservice, private router: Router) { }

  ItineraryName: string = '';
  tripDateTime: string = '';
  location: string = '';
  @Input() memberId: string = 'tw_user_001';

  @ViewChild('locationInput') locationInput!: ElementRef;

  ngAfterViewInit(): void {
    this.initGoogleAutocomplete();
  }
  placeId: string = '';
  lat: number = 0;
  lng: number = 0;
  initGoogleAutocomplete() {
    if (typeof google === 'undefined' || !this.locationInput) {
      console.error('Google API 尚未載入或找不到 locationInput');
      return;
    }

    const autocomplete = new google.maps.places.Autocomplete(
      this.locationInput.nativeElement, {
      // 限制只抓你要的資料（效能更好）
      fields: ['place_id', 'formatted_address', 'geometry'],

      // 限制台灣（可選）
      componentRestrictions: { country: 'tw' }
    }
    );
    autocomplete.addListener('place_changed', () => {
      const place = autocomplete.getPlace();
      // 防呆（非常重要）
      if (!place.geometry) {
        alert('請從下拉選單選擇地點');
        return;
      }
      this.location = place.formatted_address;
      this.placeId = place.place_id;
      this.lat = place.geometry.location.lat();
      this.lng = place.geometry.location.lng();
    });
    console.log({
      address: this.location,
      placeId: this.placeId,
      lat: this.lat,
      lng: this.lng
    });
  }


  startDateTime: string = '';
  endDateTime: string = '';
  createTrip() {
    if (this.startDateTime > this.endDateTime) {
      alert('時間錯誤');
      return;
    }
    const requestBody = {
      memberId: this.memberId,
      itineraryName: this.ItineraryName,
      startTime: new Date(this.startDateTime).toISOString(),
      endTime: new Date(this.endDateTime).toISOString(),
      introduction: '初版行程~',
      itemsToPush: [
        {
          externalLocation: {
            name: this.location,
            address: this.location,
            googlePlaceId: this.placeId,   // 👉 要從 autocomplete 拿
            latitude: this.lat,
            longitude: this.lng
          }
        }
      ]
    };
    this.mainService.createItinerary(requestBody)
      .subscribe({
        next: (res) => {
          console.log('成功:', res);
          const newId = res.id;

          if (newId) {
            // 3. 執行跳轉，帶入 ID 作為路由參數
            this.router.navigate(['/itinerary-detail', newId]);
          } else {
            alert('建立成功，但無法取得行程編號');
          }
        },
        error: (err) => {
          console.error('錯誤:', err);
          alert('建立失敗');
        }
      });
  }
  createByAI() {
    if (!this.placeId) {
      alert('請重新選擇正確的地點（需從 Google 下拉選單點擊）');
      return;
    }
    if (this.startDateTime > this.endDateTime) {
      alert('時間錯誤');
      return;
    }
    const requestBody = {
      memberId: this.memberId,
      itineraryName: this.ItineraryName,
      startTime: new Date(this.startDateTime).toISOString(),
      endTime: new Date(this.endDateTime).toISOString(),
      introduction: '初版行程~',
      itemsToPush: [
        {
          externalLocation: {
            name: this.location,
            address: this.location,
            googlePlaceId: this.placeId,   // 👉 要從 autocomplete 拿
            latitude: this.lat,
            longitude: this.lng
          }
        }
      ]
    };
    this.mainService.createAIItinerary(requestBody)
      .subscribe({
        next: (res) => {
          console.log('成功:', res);
          const newId = res.id;

          if (newId) {
            // 3. 執行跳轉，帶入 ID 作為路由參數
            this.router.navigate(['/itinerary-detail', newId]);
          } else {
            alert('建立成功，但無法取得行程編號');
          }
        },
        error: (err) => {
          console.error('錯誤:', err);
          alert('建立失敗');
        }
      });

  }
}
