import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AnalysisMetrics, VersionCompareResponse, VersionSummary } from '../interface/analysis-metrics';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class ItineraryAnalysis {
  private http = inject(HttpClient);
  baseUrl: string = environment.apiBaseUrl;
  private base = (id: number) => `${this.baseUrl}/itinerary/${id}`;

  getVersionList(itineraryId: number): Observable<VersionSummary[]> {
    return this.http.get<VersionSummary[]>(`${this.base(itineraryId)}/versions`);
  }

  getAnalysis(itineraryId: number, versionId: number): Observable<AnalysisMetrics> {
    return this.http.get<AnalysisMetrics>(
      `${this.base(itineraryId)}/versions/${versionId}/analysis`
    );
  }

  compareVersions(
    itineraryId: number, versionA: number, versionB: number
  ): Observable<VersionCompareResponse> {
    return this.http.get<VersionCompareResponse>(
      `${this.base(itineraryId)}/versions/compare`,
      { params: { versionA, versionB } }
    );
  }
}
