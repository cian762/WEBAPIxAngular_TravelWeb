import { Injectable } from '@angular/core';
import { DayItineraryItemDto } from '../interface/itinerarymainmodel';

@Injectable({
  providedIn: 'root',
})
export class GoogleMAPservice {
  private routeCache = new Map<string, google.maps.DirectionsResult>();

  calculateAndDisplayRoute(
    map: google.maps.Map,
    renderer: google.maps.DirectionsRenderer,
    itineraryId: number,
    dayNumber: number,
    items: DayItineraryItemDto[]
  ): void {
    const cacheKey = `${itineraryId}_${dayNumber}`;

    // 快取命中 → 直接顯示，不發 API 請求
    if (this.routeCache.has(cacheKey)) {
      renderer.setDirections(this.routeCache.get(cacheKey)!);
      return;
    }

    const directionsService = new google.maps.DirectionsService();
    const origin = items[0].placeId;
    const destination = items[items.length - 1].placeId;
    const waypoints = items.slice(1, -1).map(item => ({
      location: { placeId: item.placeId },
      stopover: true
    }));
    // ...發請求...
    directionsService.route({
      origin: { placeId: origin },
      destination: { placeId: destination },
      waypoints,
      travelMode: google.maps.TravelMode.DRIVING,
      optimizeWaypoints: false  // 保持使用者排定的順序
    }, (result, status) => {
      console.log('Directions status:', status); // 觀察是哪種錯誤
      console.log('Directions result:', result);  // 觀察 API 回傳的內容
      if (status === 'OK' && result) {
        this.routeCache.set(cacheKey, result); // 存入快取
        renderer.setDirections(result);
      } else {
        console.error('路線規劃失敗:', status);
      }
    });
  }

  // 使用者拖拉改變順序後，清除該天快取
  clearDayCache(itineraryId: number, dayNumber: number): void {
    this.routeCache.delete(`${itineraryId}_${dayNumber}`);
  }
}
