export interface Review {
  id: string;
  projectId: string;
  clientId: string;
  freelancerId: string;
  clientName: string;
  freelancerName: string;
  projectTitle: string;
  rating: number;
  comment: string;
  createdAtUtc: string;
}

export interface CreateReviewRequest {
  projectId: string;
  rating: number;
  comment: string;
}
