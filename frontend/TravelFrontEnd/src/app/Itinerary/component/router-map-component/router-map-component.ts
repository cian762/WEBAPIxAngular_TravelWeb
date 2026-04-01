import { Mainservice } from './../../service/mainservice';
import { AfterViewInit, Component, ElementRef, Input, OnChanges, SimpleChanges, ViewChild } from '@angular/core';
import { DayItineraryDto } from '../../interface/itinerarymainmodel';
import { GoogleMAPservice } from '../../service/google-mapservice';

declare const google: any;
@Component({
  selector: 'app-router-map-component',
  imports: [],
  templateUrl: './router-map-component.html',
  styleUrl: './router-map-component.css',
})
export class RouterMapComponent implements AfterViewInit, OnChanges {
  @Input() itineraryId!: number;
  @Input() dayNumber!: number;
  @ViewChild('mapContainer') mapContainer!: ElementRef;

  private map!: google.maps.Map;
  private renderer!: google.maps.DirectionsRenderer;
  private mapReady = false;
  constructor(private mapsService: GoogleMAPservice, private Mainservice: Mainservice) { }
  private lastLoadedKey = '';
  ngAfterViewInit(): void {
    this.map = new google.maps.Map(this.mapContainer.nativeElement, {
      zoom: 12,
      center: { lat: 25.04, lng: 121.56 }
    });
    this.renderer = new google.maps.DirectionsRenderer();
    this.renderer.setMap(this.map);
    this.mapReady = true;
    this.loadAndRenderRoute(); // ← 改這裡
  }
  ngOnChanges(changes: SimpleChanges): void {
    if (!this.mapReady) return;  // 地圖還沒好，AfterViewInit 會負責第一次載入

    const currentKey = `${this.itineraryId}_${this.dayNumber}`;
    if (currentKey === this.lastLoadedKey) return;  // ← 同樣組合不重複載入

    this.mapsService.clearDayCache(this.itineraryId, this.dayNumber);
    this.loadAndRenderRoute();
  }
  private loadAndRenderRoute(): void {
    if (!this.itineraryId || !this.dayNumber) return;
    const currentKey = `${this.itineraryId}_${this.dayNumber}`;
    this.lastLoadedKey = currentKey;  // ← 記錄這次載入的 key
    this.Mainservice.getDayItinerary(this.itineraryId, this.dayNumber)
      .subscribe({
        next: (dto) => {
          console.log('API 回傳資料：', dto);
          this.renderRoute(dto);
        },
        error: (err) => console.error('API 呼叫失敗：', err)
      });
  }
  private renderRoute(dto: DayItineraryDto): void {
    if (!dto?.items || dto.items.length < 2) {
      console.warn('地點數量不足');
      return;
    }
    const seen = new Set<string>();
    const uniqueItems = dto.items.filter(item => {
      if (!item.placeId || item.placeId === 'TEMP_AI_PLACE') return false;
      if (seen.has(item.placeId)) return false;
      seen.add(item.placeId);
      return true;
    });

    if (uniqueItems.length < 2) {
      console.warn('去重後有效 placeId 不足 2 筆');
      return;
    }

    this.mapsService.calculateAndDisplayRoute(
      this.map,
      this.renderer,
      this.itineraryId,
      this.dayNumber,
      uniqueItems
    );
  }

}
