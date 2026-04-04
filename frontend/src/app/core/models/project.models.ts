export interface Project {
  id: string;
  ownerId: string;
  title: string;
  description: string;
  budgetMin: number;
  budgetMax: number;
  status: 'Draft' | 'Open' | 'InProgress' | 'Completed' | 'Cancelled';
  assignedFreelancerId: string | null;
  assignedFreelancerName: string;
  assignedFreelancerEmail: string;
  hiredAtUtc: string | null;
  completedAtUtc: string | null;
  canReview: boolean;
  hasReview: boolean;
  createdAtUtc: string;
}

export interface CreateProjectRequest {
  title: string;
  description: string;
  budgetMin: number;
  budgetMax: number;
}

export interface HireFreelancerRequest {
  freelancerId: string;
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
  freelancerName: string;
  freelancerEmail: string;
  clientId: string;
  clientName: string;
  clientEmail: string;
  projectTitle: string;
  projectDescription: string;
  projectBudgetMin: number;
  projectBudgetMax: number;
  amount: number;
  message: string;
  status: 'Pending' | 'Accepted' | 'Rejected';
  createdAtUtc: string;
}
