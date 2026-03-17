import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';


@Injectable({
  providedIn: 'root',
})
export class Mainservice {
  private CreateItineraryapiUrl = 'https://localhost:7276/api/Itinerary';

  constructor(private http: HttpClient) { }

  createItinerary(data: any): Observable<any> {
    return this.http.post(`${this.CreateItineraryapiUrl}`, data);
  }
}
