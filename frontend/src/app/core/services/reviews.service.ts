import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { API_BASE_URL } from '../config/api.config';
import { CreateReviewRequest, Review } from '../models/review.models';

@Injectable({ providedIn: 'root' })
export class ReviewsService {
  private readonly http = inject(HttpClient);

  create(payload: CreateReviewRequest) {
    return this.http.post<Review>(`${API_BASE_URL}/reviews`, payload);
  }

  getForFreelancer(freelancerId: string) {
    return this.http.get<Review[]>(`${API_BASE_URL}/reviews/freelancer/${freelancerId}`);
  }
}
