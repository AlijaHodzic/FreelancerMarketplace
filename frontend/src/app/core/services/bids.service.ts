import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { API_BASE_URL } from '../config/api.config';
import { CreateBidRequest } from '../models/project.models';

@Injectable({ providedIn: 'root' })
export class BidsService {
  private readonly http = inject(HttpClient);

  create(payload: CreateBidRequest) {
    return this.http.post(`${API_BASE_URL}/bids`, payload);
  }
}
