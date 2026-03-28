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
  @Input() dayItinerary!: DayItineraryDto;
  @Input() itineraryId!: number;
  @Input() dayNumber!: number;
  @ViewChild('mapContainer') mapContainer!: ElementRef;

  private map!: google.maps.Map;
  private renderer!: google.maps.DirectionsRenderer;
  private mapReady = false;
  constructor(private mapsService: GoogleMAPservice) { }

  ngAfterViewInit(): void {
    this.map = new google.maps.Map(this.mapContainer.nativeElement, {
      zoom: 12,
      center: { lat: 25.04, lng: 121.56 }
    });
    this.renderer = new google.maps.DirectionsRenderer();
    this.renderer.setMap(this.map);
    this.mapReady = true;
    this.renderRoute();
  }
  ngOnChanges(changes: SimpleChanges): void {
    if (this.mapReady && changes['dayItinerary']) {
      this.renderRoute();
    }
  }
  private renderRoute(): void {
    if (!this.dayItinerary?.items || this.dayItinerary.items.length < 2) return;

    // AI 生成的地點可能 placeId 是 'TEMP_AI_PLACE'，過濾掉
    const validItems = this.dayItinerary.items.filter(
      item => item.placeId && item.placeId !== 'TEMP_AI_PLACE'
    );
    if (validItems.length < 2) return;

    this.mapsService.calculateAndDisplayRoute(
      this.map,
      this.renderer,
      this.itineraryId,
      this.dayNumber,
      validItems
    );
  }

}
