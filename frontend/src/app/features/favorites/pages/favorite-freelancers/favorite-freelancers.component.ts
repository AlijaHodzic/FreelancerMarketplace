import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CurrencyPipe, NgClass, NgFor, NgIf } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { catchError, finalize, of } from 'rxjs';
import { AuthService } from '../../../../core/auth/auth.service';
import { FreelancerSummary } from '../../../../core/models/freelancer.models';
import { Project } from '../../../../core/models/project.models';
import { FavoriteFreelancersService } from '../../../../core/services/favorite-freelancers.service';
import { ProjectsService } from '../../../../core/services/projects.service';
import { getProfileStatusBadge } from '../../../../core/utils/status-badges';

@Component({
  selector: 'app-favorite-freelancers',
  standalone: true,
  imports: [NgIf, NgFor, NgClass, RouterLink, CurrencyPipe, FormsModule],
  templateUrl: './favorite-freelancers.component.html',
  styleUrl: './favorite-freelancers.component.scss',
})
export class FavoriteFreelancersComponent implements OnInit {
  private readonly favoritesService = inject(FavoriteFreelancersService);
  private readonly projectsService = inject(ProjectsService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly favorites = signal<FreelancerSummary[]>([]);
  readonly projects = signal<Project[]>([]);
  readonly loading = signal(true);
  readonly actionFreelancerId = signal<string | null>(null);
  readonly inviteFreelancerId = signal<string | null>(null);
  readonly selectedProjectId = signal<string>('');
  readonly searchTerm = signal('');
  readonly errorMessage = signal('');

  readonly canInvite = computed(() => this.authService.role() === 'Client' || this.authService.role() === 'Admin');

  readonly filteredFavorites = computed(() => {
    const query = this.searchTerm().trim().toLowerCase();
    const favorites = this.favorites();

    if (!query) {
      return favorites;
    }

    return favorites.filter((freelancer) =>
      [freelancer.name, freelancer.title, freelancer.category, freelancer.location, ...freelancer.skills]
        .join(' ')
        .toLowerCase()
        .includes(query),
    );
  });

  readonly profileStatusBadge = getProfileStatusBadge;

  ngOnInit() {
    this.loadFavorites();

    if (this.canInvite()) {
      this.projectsService.getMine().pipe(catchError(() => of([] as Project[]))).subscribe((projects) => this.projects.set(projects));
    }
  }

  openInvite(freelancerId: string) {
    this.inviteFreelancerId.set(freelancerId);
    this.selectedProjectId.set(this.projects()[0]?.id ?? '');
  }

  closeInvite() {
    this.inviteFreelancerId.set(null);
    this.selectedProjectId.set('');
  }

  inviteToJob(freelancer: FreelancerSummary) {
    const project = this.projects().find((item) => item.id === this.selectedProjectId());
    if (!project) {
      this.errorMessage.set('Create or load a client job first so you can send an invite.');
      return;
    }

    void this.router.navigate(['/messages'], {
      queryParams: {
        recipientEmail: freelancer.email,
        subject: `Invitation to ${project.title}`,
        initialMessage: `Hi ${freelancer.name}, I'd like to invite you to my job "${project.title}". Budget: $${project.budgetMin} - $${project.budgetMax}. Let me know if you're interested.`,
      },
    });
  }

  removeFavorite(freelancerId: string) {
    this.actionFreelancerId.set(freelancerId);
    this.errorMessage.set('');

    this.favoritesService
      .remove(freelancerId)
      .pipe(
        finalize(() => this.actionFreelancerId.set(null)),
        catchError((error) => {
          this.errorMessage.set(error.error?.message ?? 'We could not remove this freelancer right now.');
          return of(null);
        }),
      )
      .subscribe((result) => {
        if (result === null) {
          return;
        }

        this.favorites.update((favorites) => favorites.filter((freelancer) => freelancer.id !== freelancerId));
        if (this.inviteFreelancerId() === freelancerId) {
          this.closeInvite();
        }
      });
  }

  private loadFavorites() {
    this.loading.set(true);
    this.errorMessage.set('');

    this.favoritesService
      .getMine()
      .pipe(
        finalize(() => this.loading.set(false)),
        catchError((error) => {
          this.errorMessage.set(error.error?.message ?? 'We could not load your saved freelancers right now.');
          return of([] as FreelancerSummary[]);
        }),
      )
      .subscribe((favorites) => this.favorites.set(favorites));
  }
}
