import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { API_BASE_URL } from '../config/api.config';
import { FreelancerSummary } from '../models/freelancer.models';

@Injectable({ providedIn: 'root' })
export class FavoriteFreelancersService {
  private readonly http = inject(HttpClient);

  getMine() {
    return this.http.get<FreelancerSummary[]>(`${API_BASE_URL}/favorites/freelancers`);
  }

  add(freelancerId: string) {
    return this.http.post<void>(`${API_BASE_URL}/favorites/freelancers/${freelancerId}`, {});
  }

  remove(freelancerId: string) {
    return this.http.delete<void>(`${API_BASE_URL}/favorites/freelancers/${freelancerId}`);
  }
}
