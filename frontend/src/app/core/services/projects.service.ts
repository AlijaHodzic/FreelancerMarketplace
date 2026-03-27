import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { API_BASE_URL } from '../config/api.config';
import { CreateProjectRequest, Project } from '../models/project.models';

@Injectable({ providedIn: 'root' })
export class ProjectsService {
  private readonly http = inject(HttpClient);

  getAll() {
    return this.http.get<Project[]>(`${API_BASE_URL}/projects`);
  }

  create(payload: CreateProjectRequest) {
    return this.http.post<Project>(`${API_BASE_URL}/projects`, payload);
  }
}
