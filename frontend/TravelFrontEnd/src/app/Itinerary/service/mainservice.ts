import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { DayItineraryDto } from '../interface/itinerarymainmodel';


@Injectable({
  providedIn: 'root',
})
export class Mainservice {
  baseUrl: string = environment.apiBaseUrl;

  private CreateItineraryapiUrl = `${this.baseUrl}/Itinerary`;
  private CreateAIItineraryAPI = `${this.baseUrl}/AiItinerary/generate-ai`;
  private GetDayItineraryAPI = `${this.baseUrl}/Itinerary/{itineraryId}/day/{day}`;
  constructor(private http: HttpClient) { }

  createItinerary(data: any): Observable<any> {
    return this.http.post(`${this.CreateItineraryapiUrl}`, data);
  }
  createAIItinerary(data: any): Observable<any> {
    return this.http.post(`${this.CreateAIItineraryAPI}`, data)
  }
  getDayItinerary(itineraryId: number, day: number): Observable<DayItineraryDto> {
    return this.http.get<DayItineraryDto>(
      `${this.baseUrl}/itinerary/${itineraryId}/day/${day}`
    );
  }
}
