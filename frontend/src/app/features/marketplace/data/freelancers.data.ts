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

export interface FreelancerProfile {
  slug: string;
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
  about: string;
  skills: string[];
  description: string;
  portfolio: FreelancerPortfolioItem[];
  testimonials: FreelancerReview[];
}

export const FREELANCERS: FreelancerProfile[] = [
  {
    slug: 'alex-rivera',
    avatar: 'https://i.pravatar.cc/160?img=12',
    name: 'Alex Rivera',
    title: 'Full Stack Developer',
    rating: 4.9,
    reviews: 127,
    hourlyRate: 85,
    location: 'San Francisco, CA',
    category: 'Web Development',
    experience: 'Expert',
    responseTime: '1 hour',
    successRate: '98%',
    completedProjects: 156,
    description: 'Builds scalable web platforms, polished client portals, and production-ready dashboard experiences.',
    about:
      'Full-stack developer with 8+ years of experience building scalable web applications. Specialized in modern JavaScript frameworks and cloud architecture. Passionate about clean code, performance, and user-centric product design.',
    skills: ['React', 'Node.js', 'TypeScript', 'PostgreSQL', 'AWS'],
    portfolio: [
      {
        title: 'E-commerce Platform',
        summary: 'Built a complete e-commerce solution with React and Node.js, handling 10k+ daily transactions.',
        image: 'https://images.unsplash.com/photo-1523275335684-37898b6baf30?auto=format&fit=crop&w=900&q=80',
        tags: ['React', 'Node.js', 'Stripe'],
      },
      {
        title: 'SaaS Dashboard',
        summary: 'Developed an analytics dashboard for a B2B SaaS company with real-time data visualization.',
        image: 'https://images.unsplash.com/photo-1551288049-bebda4e38f71?auto=format&fit=crop&w=900&q=80',
        tags: ['TypeScript', 'D3.js', 'PostgreSQL'],
      },
      {
        title: 'Mobile Banking App',
        summary: 'Created a secure mobile banking application with biometric authentication and audit-ready flows.',
        image: 'https://images.unsplash.com/photo-1516321318423-f06f85e504b3?auto=format&fit=crop&w=900&q=80',
        tags: ['React Native', 'AWS', 'Security'],
      },
    ],
    testimonials: [
      {
        author: 'David Park',
        project: 'E-commerce Platform Development',
        date: 'Dec 15, 2025',
        rating: 5,
        comment:
          'Alex delivered exceptional work on our e-commerce platform. Great communication, technical expertise, and attention to detail. Highly recommend!',
        avatar: 'https://i.pravatar.cc/80?img=21',
      },
      {
        author: 'Lisa Martinez',
        project: 'SaaS Dashboard Development',
        date: 'Nov 28, 2025',
        rating: 5,
        comment:
          'Outstanding developer! Alex understood our requirements perfectly and delivered ahead of schedule. Will definitely work together again.',
        avatar: 'https://i.pravatar.cc/80?img=31',
      },
      {
        author: 'James Wilson',
        project: 'API Integration',
        date: 'Nov 10, 2025',
        rating: 5,
        comment:
          'Professional, skilled, and reliable. Alex went above and beyond to ensure our project was a success.',
        avatar: 'https://i.pravatar.cc/80?img=41',
      },
    ],
  },
  {
    slug: 'amina-kovac',
    avatar: 'https://i.pravatar.cc/160?img=22',
    name: 'Amina Kovac',
    title: 'Senior Product Designer',
    rating: 4.9,
    reviews: 94,
    hourlyRate: 32,
    location: 'Sarajevo, BiH',
    category: 'Product Design',
    experience: 'Senior',
    responseTime: '2 hours',
    successRate: '97%',
    completedProjects: 84,
    description: 'Creates polished SaaS flows, mobile onboarding, and conversion-focused landing pages.',
    about:
      'Product designer focused on SaaS interfaces, onboarding systems, and clean visual storytelling. Loves turning unclear product ideas into intuitive journeys and design systems.',
    skills: ['Figma', 'Design Systems', 'UX Research'],
    portfolio: [],
    testimonials: [],
  },
  {
    slug: 'luka-marin',
    avatar: 'https://i.pravatar.cc/160?img=32',
    name: 'Luka Marin',
    title: 'Full-Stack Developer',
    rating: 4.8,
    reviews: 67,
    hourlyRate: 45,
    location: 'Split, HR',
    category: 'Web Development',
    experience: 'Senior',
    responseTime: '3 hours',
    successRate: '96%',
    completedProjects: 71,
    description: 'Builds production-ready dashboards, API integrations, and scalable marketplace features.',
    about:
      'Full-stack engineer experienced in Angular, Node.js, and backend-heavy business systems with a strong eye for maintainability.',
    skills: ['Angular', 'Node.js', 'PostgreSQL'],
    portfolio: [],
    testimonials: [],
  },
  {
    slug: 'lejla-hadzic',
    avatar: 'https://i.pravatar.cc/160?img=42',
    name: 'Lejla Hadzic',
    title: 'Content Strategist',
    rating: 4.7,
    reviews: 156,
    hourlyRate: 24,
    location: 'Tuzla, BiH',
    category: 'Content',
    experience: 'Expert',
    responseTime: '4 hours',
    successRate: '95%',
    completedProjects: 133,
    description: 'Writes conversion copy and content systems tailored for startups and service businesses.',
    about:
      'Content strategist helping startups clarify their offer, improve acquisition funnels, and build consistent brand messaging.',
    skills: ['SEO', 'Copywriting', 'Content Plans'],
    portfolio: [],
    testimonials: [],
  },
  {
    slug: 'mia-petrovic',
    avatar: 'https://i.pravatar.cc/160?img=52',
    name: 'Mia Petrovic',
    title: 'Mobile App Developer',
    rating: 4.9,
    reviews: 121,
    hourlyRate: 52,
    location: 'Belgrade, RS',
    category: 'Mobile Development',
    experience: 'Expert',
    responseTime: '1 hour',
    successRate: '98%',
    completedProjects: 109,
    description: 'Helps startups launch reliable mobile products with strong UX and production-ready code.',
    about:
      'Mobile engineer focused on React Native product delivery, polished release cycles, and startup MVP acceleration.',
    skills: ['React Native', 'iOS', 'Android'],
    portfolio: [],
    testimonials: [],
  },
  {
    slug: 'omar-selimovic',
    avatar: 'https://i.pravatar.cc/160?img=62',
    name: 'Omar Selimovic',
    title: 'Data & Automation Specialist',
    rating: 4.8,
    reviews: 81,
    hourlyRate: 38,
    location: 'Mostar, BiH',
    category: 'Automation',
    experience: 'Senior',
    responseTime: '2 hours',
    successRate: '97%',
    completedProjects: 76,
    description: 'Builds internal tools, reporting flows, and automations that save teams hours each week.',
    about:
      'Automation-focused engineer working across Python, dashboards, and operations tooling for growing teams.',
    skills: ['Python', 'Dashboards', 'Automation'],
    portfolio: [],
    testimonials: [],
  },
  {
    slug: 'nina-basic',
    avatar: 'https://i.pravatar.cc/160?img=72',
    name: 'Nina Basic',
    title: 'Brand Designer',
    rating: 4.9,
    reviews: 112,
    hourlyRate: 29,
    location: 'Zagreb, HR',
    category: 'Brand Design',
    experience: 'Senior',
    responseTime: '5 hours',
    successRate: '99%',
    completedProjects: 98,
    description: 'Creates clean visual identities, marketing assets, and launch-ready social media packs.',
    about:
      'Brand designer helping service businesses and startups show up with stronger visual systems and clearer presentation.',
    skills: ['Branding', 'Illustration', 'Social Media'],
    portfolio: [],
    testimonials: [],
  },
];
