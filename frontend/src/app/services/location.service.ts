import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface LocationResponse {
  count: number;
  locations: Array<{ location_Code: string; location_Name: string }>;
}

@Injectable({
  providedIn: 'root'
})
export class LocationService {
  private readonly baseUrl = 'http://localhost:5007/api';

  constructor(private readonly http: HttpClient) {}

  getLocations(): Observable<LocationResponse> {
    return this.http.get<LocationResponse>(`${this.baseUrl}/locations`);
  }
}
