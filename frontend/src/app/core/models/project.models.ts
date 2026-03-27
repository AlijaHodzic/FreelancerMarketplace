export interface Project {
  id: string;
  title: string;
  description: string;
  budgetMin: number;
  budgetMax: number;
  status: 'Open' | 'InProgress' | 'Completed' | 'Cancelled';
  createdAtUtc: string;
}

export interface CreateProjectRequest {
  title: string;
  description: string;
  budgetMin: number;
  budgetMax: number;
}

export interface CreateBidRequest {
  projectId: string;
  amount: number;
  message: string;
}
