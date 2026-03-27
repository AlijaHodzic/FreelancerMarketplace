import { Component } from '@angular/core';
import { NgFor } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-how-it-works',
  standalone: true,
  imports: [NgFor, RouterLink],
  templateUrl: './how-it-works.component.html',
  styleUrl: './how-it-works.component.scss',
})
export class HowItWorksComponent {
  readonly clientSteps = [
    {
      title: 'Post your brief',
      description: 'Describe the scope, budget, and timeline so freelancers can send relevant proposals fast.',
    },
    {
      title: 'Review matched talent',
      description: 'Compare portfolios, rates, and reviews to shortlist people who fit your project best.',
    },
    {
      title: 'Collaborate confidently',
      description: 'Manage milestones, feedback, and payments in one flow without losing project visibility.',
    },
  ] as const;

  readonly freelancerSteps = [
    {
      title: 'Create a profile',
      description: 'Showcase your skills, experience, and portfolio to attract better-fit opportunities.',
    },
    {
      title: 'Apply to jobs',
      description: 'Send targeted proposals and respond quickly when new projects match your expertise.',
    },
    {
      title: 'Get paid',
      description: 'Complete milestones, deliver work, and receive secure payments through the platform.',
    },
  ] as const;
}
