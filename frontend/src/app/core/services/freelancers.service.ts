import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { API_BASE_URL } from '../config/api.config';
import { FreelancerProfile, FreelancerSummary, UpdateFreelancerProfileRequest } from '../models/freelancer.models';

@Injectable({ providedIn: 'root' })
export class FreelancersService {
  private readonly http = inject(HttpClient);

  getAll() {
    return this.http.get<FreelancerSummary[]>(`${API_BASE_URL}/freelancers`);
  }

  getBySlug(slug: string) {
    return this.http.get<FreelancerProfile>(`${API_BASE_URL}/freelancers/${slug}`);
  }

  getMineProfile() {
    return this.http.get<FreelancerProfile>(`${API_BASE_URL}/freelancers/me/profile`);
  }

  updateMineProfile(payload: UpdateFreelancerProfileRequest) {
    return this.http.put<FreelancerProfile>(`${API_BASE_URL}/freelancers/me/profile`, payload);
  }
}
