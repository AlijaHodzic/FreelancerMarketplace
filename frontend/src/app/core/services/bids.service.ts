import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { API_BASE_URL } from '../config/api.config';
import { Bid, CreateBidRequest } from '../models/project.models';

@Injectable({ providedIn: 'root' })
export class BidsService {
  private readonly http = inject(HttpClient);

  create(payload: CreateBidRequest) {
    return this.http.post<Bid>(`${API_BASE_URL}/bids`, payload);
  }

  getMine() {
    return this.http.get<Bid[]>(`${API_BASE_URL}/bids/mine`);
  }

  getByProject(projectId: string) {
    return this.http.get<Bid[]>(`${API_BASE_URL}/bids/project/${projectId}`);
  }

  accept(bidId: string) {
    return this.http.post<void>(`${API_BASE_URL}/bids/${bidId}/accept`, {});
  }

  reject(bidId: string) {
    return this.http.post<void>(`${API_BASE_URL}/bids/${bidId}/reject`, {});
  }
}
