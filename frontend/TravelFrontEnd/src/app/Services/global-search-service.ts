import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';


@Injectable({
  providedIn: 'root',
})
export class GlobalSearchService {

  private baseUrl = environment.apiBaseUrl;
  private apiUrl = `${this.baseUrl}/GlobalSearch`;

  constructor(private http: HttpClient) { }

  getSearchResults(q: string): Observable<any[]> {
    const params = new HttpParams().set('q', q);
    return this.http.get<any[]>(this.apiUrl, { params });
  }

  getSuggestions(q: string): Observable<string[]> {
    const params = new HttpParams().set('q', q);
    return this.http.get<string[]>(`${this.apiUrl}/suggestions`, { params });
  }

}
