import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';


@Injectable({
  providedIn: 'root',
})
export class Mainservice {
  baseUrl: string = environment.apiBaseUrl;

  private CreateItineraryapiUrl = `${this.baseUrl}/Itinerary`;
  private CreateAIItineraryAPI = `${this.baseUrl}/AiItinerary/generate-ai`;
  constructor(private http: HttpClient) { }

  createItinerary(data: any): Observable<any> {
    return this.http.post(`${this.CreateItineraryapiUrl}`, data);
  }
  createAIItinerary(data: any): Observable<any> {
    return this.http.post(`${this.CreateAIItineraryAPI}`, data)
  }
}
