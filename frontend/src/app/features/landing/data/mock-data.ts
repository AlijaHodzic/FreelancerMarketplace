export type CategoryIcon =
  | 'Code'
  | 'Smartphone'
  | 'Palette'
  | 'Paintbrush'
  | 'FileText'
  | 'TrendingUp'
  | 'Video'
  | 'Database';

export interface Category {
  id: string;
  name: string;
  count: number;
  icon: CategoryIcon;
}


export interface Testimonial {
  id: string;
  name: string;
  role: string;
  company: string;
  content: string;
  avatar: string;
  rating: number;
}


export const categories: Category[] = [
  { id: '1', name: 'Web Development', count: 3120, icon: 'Code' },
  { id: '2', name: 'Mobile Apps', count: 1840, icon: 'Smartphone' },
  { id: '3', name: 'UI/UX Design', count: 2260, icon: 'Palette' },
  { id: '4', name: 'Graphic Design', count: 1670, icon: 'Paintbrush' },
  { id: '5', name: 'Content Writing', count: 990, icon: 'FileText' },
  { id: '6', name: 'Marketing', count: 1210, icon: 'TrendingUp' },
  { id: '7', name: 'Video Editing', count: 760, icon: 'Video' },
  { id: '8', name: 'Data', count: 540, icon: 'Database' },
];


export const testimonials: Testimonial[] = [
  {
    id: 't1',
    name: 'Sarah M.',
    role: 'Founder',
    company: 'Startup Inc.',
    content: 'Great experience. Fast delivery and top quality.',
    avatar: 'https://i.pravatar.cc/150?img=32',
    rating: 5,
  },
  {
    id: 't2',
    name: 'Mark L.',
    role: 'Product Manager',
    company: 'SaaS Corp',
    content: 'FreelanceHub made hiring simple and reliable.',
    avatar: 'https://i.pravatar.cc/150?img=12',
    rating: 5,
  },
  {
    id: 't3',
    name: 'Amina K.',
    role: 'CEO',
    company: 'Agency',
    content: 'We found excellent talent within hours.',
    avatar: 'https://i.pravatar.cc/150?img=48',
    rating: 4,
  },
];

