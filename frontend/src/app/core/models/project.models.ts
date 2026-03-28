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

export interface Bid {
  id: string;
  projectId: string;
  freelancerId: string;
  projectTitle: string;
  projectDescription: string;
  projectBudgetMin: number;
  projectBudgetMax: number;
  amount: number;
  message: string;
  status: 'Pending' | 'Accepted' | 'Rejected';
  createdAtUtc: string;
}
