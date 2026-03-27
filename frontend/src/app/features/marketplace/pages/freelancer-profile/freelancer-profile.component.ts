import { Component, computed, inject } from '@angular/core';
import { CurrencyPipe, NgFor, NgIf } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FREELANCERS } from '../../data/freelancers.data';

@Component({
  selector: 'app-freelancer-profile',
  standalone: true,
  imports: [NgIf, NgFor, RouterLink, CurrencyPipe],
  templateUrl: './freelancer-profile.component.html',
  styleUrl: './freelancer-profile.component.scss',
})
export class FreelancerProfileComponent {
  private readonly route = inject(ActivatedRoute);

  readonly freelancer = computed(() => {
    const slug = this.route.snapshot.paramMap.get('slug');
    return FREELANCERS.find((candidate) => candidate.slug === slug) ?? null;
  });

  readonly stars = Array.from({ length: 5 });
}
