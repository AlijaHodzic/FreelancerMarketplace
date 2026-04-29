import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { API_BASE_URL } from '../config/api.config';
import { AdminActivity, AdminProject, AdminSummary, AdminUser } from '../models/admin.models';

@Injectable({ providedIn: 'root' })
export class AdminService {
  private readonly http = inject(HttpClient);

  getSummary() {
    return this.http.get<AdminSummary>(`${API_BASE_URL}/admin/summary`);
  }

  getUsers() {
    return this.http.get<AdminUser[]>(`${API_BASE_URL}/admin/users`);
  }

  getProjects() {
    return this.http.get<AdminProject[]>(`${API_BASE_URL}/admin/projects`);
  }

  getActivity() {
    return this.http.get<AdminActivity[]>(`${API_BASE_URL}/admin/activity`);
  }
}
