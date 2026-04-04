export interface FreelancerPortfolioItem {
  title: string;
  summary: string;
  image: string;
  tags: string[];
}

export interface FreelancerReview {
  author: string;
  project: string;
  date: string;
  rating: number;
  comment: string;
  avatar: string;
}

export interface FreelancerSummary {
  id: string;
  slug: string;
  email: string;
  avatar: string;
  name: string;
  title: string;
  rating: number;
  reviews: number;
  hourlyRate: number;
  location: string;
  category: string;
  experience: string;
  responseTime: string;
  successRate: string;
  completedProjects: number;
  description: string;
  skills: string[];
}

export interface FreelancerProfile extends FreelancerSummary {
  about: string;
  portfolio: FreelancerPortfolioItem[];
  testimonials: FreelancerReview[];
}

export interface UpdateFreelancerProfileRequest {
  fullName: string;
  headline: string;
  avatar: string;
  location: string;
  category: string;
  experience: string;
  responseTime: string;
  successRate: string;
  completedProjects: number;
  hourlyRate: number;
  description: string;
  about: string;
  skills: string[];
  portfolio: FreelancerPortfolioItem[];
}
