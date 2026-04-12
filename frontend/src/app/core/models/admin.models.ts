export interface AdminSummary {
  totalUsers: number;
  totalClients: number;
  totalFreelancers: number;
  totalAdmins: number;
  totalProjects: number;
  openProjects: number;
  inProgressProjects: number;
  completedProjects: number;
  totalBids: number;
  pendingBids: number;
  acceptedBids: number;
  rejectedBids: number;
  totalConversations: number;
  totalMessages: number;
  totalReviews: number;
  totalNotifications: number;
  recentUsers: AdminUser[];
  recentProjects: AdminProject[];
}

export interface AdminUser {
  id: string;
  fullName: string;
  email: string;
  role: 'Admin' | 'Client' | 'Freelancer';
  location: string;
  createdAtUtc: string;
}

export interface AdminProject {
  id: string;
  title: string;
  clientName: string;
  status: 'Draft' | 'Open' | 'InProgress' | 'Completed' | 'Cancelled';
  budgetMin: number;
  budgetMax: number;
  bidsCount: number;
  createdAtUtc: string;
}
