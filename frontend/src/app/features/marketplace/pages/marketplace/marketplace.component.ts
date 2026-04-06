import { Component, OnInit, PLATFORM_ID, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgClass, NgFor, NgIf, isPlatformBrowser } from '@angular/common';
import { RouterLink } from '@angular/router';
import { catchError, finalize, of } from 'rxjs';
import { AuthService } from '../../../../core/auth/auth.service';
import { FreelancerSummary } from '../../../../core/models/freelancer.models';
import { FavoriteFreelancersService } from '../../../../core/services/favorite-freelancers.service';
import { FreelancersService } from '../../../../core/services/freelancers.service';
import { getProfileStatusBadge } from '../../../../core/utils/status-badges';

@Component({
  selector: 'app-marketplace',
  standalone: true,
  imports: [NgFor, NgIf, NgClass, RouterLink, FormsModule],
  templateUrl: './marketplace.component.html',
  styleUrl: './marketplace.component.scss',
})
export class MarketplaceComponent implements OnInit {
  private readonly platformId = inject(PLATFORM_ID);
  private readonly freelancersService = inject(FreelancersService);
  private readonly favoriteFreelancersService = inject(FavoriteFreelancersService);
  private readonly authService = inject(AuthService);

  readonly filters = ['Web Development', 'Product Design', 'Content', 'Mobile Development', 'Automation', 'Brand Design'];
  readonly allFilters = ['All', ...this.filters];
  readonly freelancers = signal<FreelancerSummary[]>([]);
  readonly loading = signal(true);
  readonly savingFreelancerId = signal<string | null>(null);
  readonly errorMessage = signal('');
  readonly searchTerm = signal('');
  readonly activeCategory = signal('All');
  readonly sortBy = signal<'best-match' | 'highest-rate' | 'fast-response'>('best-match');
  readonly savedFreelancerIds = signal<string[]>([]);

  readonly canSaveFreelancers = computed(() => this.authService.role() === 'Client' || this.authService.role() === 'Admin');
  readonly profileStatusBadge = getProfileStatusBadge;

  readonly filteredFreelancers = computed(() => {
    const query = this.searchTerm().trim().toLowerCase();
    const category = this.activeCategory();
    const sortBy = this.sortBy();
    let freelancers = this.freelancers();

    if (category !== 'All') {
      freelancers = freelancers.filter((freelancer) => freelancer.category === category);
    }

    if (query) {
      freelancers = freelancers.filter((freelancer) =>
        [freelancer.name, freelancer.title, freelancer.location, freelancer.category, freelancer.description, ...freelancer.skills]
          .join(' ')
          .toLowerCase()
          .includes(query),
      );
    }

    return [...freelancers].sort((left, right) => {
      switch (sortBy) {
        case 'highest-rate':
          return right.hourlyRate - left.hourlyRate;
        case 'fast-response':
          return left.responseTime.localeCompare(right.responseTime);
        default:
          return right.rating - left.rating || right.completedProjects - left.completedProjects;
      }
    });
  });

  ngOnInit() {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    this.freelancersService
      .getAll()
      .pipe(
        finalize(() => this.loading.set(false)),
        catchError((error) => {
          this.errorMessage.set(error.error?.message ?? 'We could not load freelancers right now.');
          return of([] as FreelancerSummary[]);
        }),
      )
      .subscribe((freelancers) => this.freelancers.set(freelancers));

    if (this.canSaveFreelancers()) {
      this.favoriteFreelancersService
        .getMine()
        .pipe(catchError(() => of([] as FreelancerSummary[])))
        .subscribe((favorites) => this.savedFreelancerIds.set(favorites.map((freelancer) => freelancer.id)));
    }
  }

  setCategory(category: string) {
    this.activeCategory.set(category);
  }

  isSaved(freelancerId: string) {
    return this.savedFreelancerIds().includes(freelancerId);
  }

  toggleSaved(freelancer: FreelancerSummary) {
    if (!this.canSaveFreelancers()) {
      return;
    }

    const wasSaved = this.isSaved(freelancer.id);
    const request$ = wasSaved
      ? this.favoriteFreelancersService.remove(freelancer.id)
      : this.favoriteFreelancersService.add(freelancer.id);

    this.savingFreelancerId.set(freelancer.id);
    this.errorMessage.set('');

    request$
      .pipe(
        finalize(() => this.savingFreelancerId.set(null)),
        catchError((error) => {
          this.errorMessage.set(error.error?.message ?? 'We could not update saved freelancers right now.');
          return of(null);
        }),
      )
      .subscribe((result) => {
        if (result === null) {
          return;
        }

        this.savedFreelancerIds.update((ids) => (wasSaved ? ids.filter((id) => id !== freelancer.id) : [...ids, freelancer.id]));
      });
  }
}
