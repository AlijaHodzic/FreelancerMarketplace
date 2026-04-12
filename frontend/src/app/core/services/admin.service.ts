import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { API_BASE_URL } from '../config/api.config';
import { AdminSummary } from '../models/admin.models';

@Injectable({ providedIn: 'root' })
export class AdminService {
  private readonly http = inject(HttpClient);

  getSummary() {
    return this.http.get<AdminSummary>(`${API_BASE_URL}/admin/summary`);
  }
}
