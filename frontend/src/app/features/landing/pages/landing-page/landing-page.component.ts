import { Component, inject } from '@angular/core';
import { NgFor, NgSwitch, NgSwitchCase } from '@angular/common';
import { Router } from '@angular/router';
import { categories, testimonials } from '../../data/mock-data';
import { CategoryCardComponent } from '../../components/category-card/category-card.component';
import { TestimonialCardComponent } from '../../components/testimonial-card/testimonial-card.component';

@Component({
  selector: 'app-landing-page',
  standalone: true,
  imports: [NgFor, CategoryCardComponent, TestimonialCardComponent, NgSwitch, NgSwitchCase],
  templateUrl: './landing-page.component.html',
  styleUrls: ['./landing-page.component.scss'],
})
export class LandingPageComponent {
  private readonly router = inject(Router);

  categories = categories;
  testimonials = testimonials;

  popularTerms = ['React Developer', 'UI/UX Designer', 'Content Writer', 'Video Editor'];

  stats = [
    { label: 'Active Freelancers', value: '10,000+' },
    { label: 'Projects Completed', value: '50,000+' },
    { label: 'Client Satisfaction', value: '98%' },
    { label: 'Countries', value: '150+' },
  ] as const;

  features = [
    {
      key: 'talent',
      title: 'Top Talent',
      description: 'Access thousands of vetted freelancers with verified skills and portfolios.',
      icon: 'users',
    },
    {
      key: 'payments',
      title: 'Secure Payments',
      description: 'Safe and reliable payment system with milestone-based releases and escrow protection.',
      icon: 'shield',
    },
    {
      key: 'fast',
      title: 'Fast Hiring',
      description: 'Post your project and receive proposals within hours. Start working immediately.',
      icon: 'zap',
    },
    {
      key: 'time',
      title: 'Time Tracking',
      description: 'Built-in time tracking and project management tools for seamless collaboration.',
      icon: 'clock',
    },
    {
      key: 'qa',
      title: 'Quality Assurance',
      description: "Review work before payment. Request revisions until you're 100% satisfied.",
      icon: 'check',
    },
    {
      key: 'support',
      title: '24/7 Support',
      description: 'Dedicated support team ready to help you succeed at every step.',
      icon: 'users',
    },
  ] as const;

  goTo(route: '/marketplace' | '/post-job' | '/register') {
    this.router.navigateByUrl(route);
  }
}
