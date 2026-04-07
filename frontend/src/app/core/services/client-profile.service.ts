import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { API_BASE_URL } from '../config/api.config';
import { ClientProfile, UpdateClientProfileRequest } from '../models/client-profile.models';

@Injectable({ providedIn: 'root' })
export class ClientProfileService {
  private readonly http = inject(HttpClient);

  getMineProfile() {
    return this.http.get<ClientProfile>(`${API_BASE_URL}/clients/me/profile`);
  }

  updateMineProfile(payload: UpdateClientProfileRequest) {
    return this.http.put<ClientProfile>(`${API_BASE_URL}/clients/me/profile`, payload);
  }
}
