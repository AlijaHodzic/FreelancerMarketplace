export interface ClientProfile {
  id: string;
  email: string;
  fullName: string;
  headline: string;
  avatar: string;
  location: string;
  companyName: string;
  companyDescription: string;
  about: string;
}

export interface UpdateClientProfileRequest {
  fullName: string;
  headline: string;
  avatar: string;
  location: string;
  companyName: string;
  companyDescription: string;
  about: string;
}
