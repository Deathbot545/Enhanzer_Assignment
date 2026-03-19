import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface PurchaseBillRequest {
  billNumber: string;
  supplierName: string;
  billDate: string;
  amount: number;
  remarks?: string;
}

interface PurchaseBillResponse {
  message: string;
  id: number;
}

@Injectable({
  providedIn: 'root'
})
export class PurchaseBillService {
  private readonly baseUrl = 'http://localhost:5007/api';

  constructor(private readonly http: HttpClient) {}

  add(request: PurchaseBillRequest): Observable<PurchaseBillResponse> {
    return this.http.post<PurchaseBillResponse>(`${this.baseUrl}/purchasebills`, request);
  }
}
