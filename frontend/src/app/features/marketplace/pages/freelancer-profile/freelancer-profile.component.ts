import { Component, OnInit, PLATFORM_ID, computed, inject, signal } from '@angular/core';
import { CurrencyPipe, DatePipe, NgClass, NgFor, NgIf, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { catchError, finalize, of } from 'rxjs';
import { AuthService } from '../../../../core/auth/auth.service';
import { FreelancerProfile } from '../../../../core/models/freelancer.models';
import { HireFreelancerRequest, Project } from '../../../../core/models/project.models';
import { Review } from '../../../../core/models/review.models';
import { FavoriteFreelancersService } from '../../../../core/services/favorite-freelancers.service';
import { FreelancersService } from '../../../../core/services/freelancers.service';
import { ProjectsService } from '../../../../core/services/projects.service';
import { ReviewsService } from '../../../../core/services/reviews.service';
import { getProfileStatusBadge } from '../../../../core/utils/status-badges';

@Component({
  selector: 'app-freelancer-profile',
  standalone: true,
  imports: [NgIf, NgFor, NgClass, RouterLink, CurrencyPipe, DatePipe, FormsModule],
  templateUrl: './freelancer-profile.component.html',
  styleUrl: './freelancer-profile.component.scss',
})
export class FreelancerProfileComponent implements OnInit {
  private readonly platformId = inject(PLATFORM_ID);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly freelancersService = inject(FreelancersService);
  private readonly favoriteFreelancersService = inject(FavoriteFreelancersService);
  private readonly projectsService = inject(ProjectsService);
  private readonly reviewsService = inject(ReviewsService);
  private readonly authService = inject(AuthService);

  readonly freelancer = signal<FreelancerProfile | null>(null);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly hiring = signal(false);
  readonly inviteOpen = signal(false);
  readonly hireNowOpen = signal(false);
  readonly errorMessage = signal('');
  readonly selectedProjectId = signal('');
  readonly projects = signal<Project[]>([]);
  readonly reviews = signal<Review[]>([]);
  readonly stars = Array.from({ length: 5 });
  readonly savedFreelancerIds = signal<string[]>([]);
  readonly hireForm = signal<HireFreelancerRequest>({
    freelancerId: '',
    title: '',
    description: '',
    budgetMin: 1000,
    budgetMax: 1500,
  });

  readonly canSaveFreelancers = computed(() => this.authService.role() === 'Client' || this.authService.role() === 'Admin');
  readonly canInvite = this.canSaveFreelancers;
  readonly canHire = this.canSaveFreelancers;
  readonly profileStatusBadge = getProfileStatusBadge;

  ngOnInit() {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    const slug = this.route.snapshot.paramMap.get('slug');

    if (!slug) {
      this.loading.set(false);
      this.errorMessage.set('Freelancer not found');
      return;
    }

    this.freelancersService
      .getBySlug(slug)
      .pipe(
        finalize(() => this.loading.set(false)),
        catchError((error) => {
          this.errorMessage.set(error.error?.message ?? 'We could not load this freelancer right now.');
          return of(null);
        }),
      )
      .subscribe((freelancer) => {
        this.freelancer.set(freelancer);

        if (freelancer) {
          this.hireForm.set({
            freelancerId: freelancer.id,
            title: `${freelancer.name} direct engagement`,
            description: `Direct hire for ${freelancer.title}. Define the scope, milestones, and expected delivery here.`,
            budgetMin: Math.max(Math.round(freelancer.hourlyRate * 20), 500),
            budgetMax: Math.max(Math.round(freelancer.hourlyRate * 30), 800),
          });

          this.reviewsService
            .getForFreelancer(freelancer.id)
            .pipe(catchError(() => of([] as Review[])))
            .subscribe((reviews) => this.reviews.set(reviews));
        }
      });

    if (this.canSaveFreelancers()) {
      this.favoriteFreelancersService
        .getMine()
        .pipe(catchError(() => of([] as FreelancerProfile[])))
        .subscribe((favorites) => this.savedFreelancerIds.set(favorites.map((freelancer) => freelancer.id)));

      this.projectsService.getMine().pipe(catchError(() => of([] as Project[]))).subscribe((projects) => this.projects.set(projects));
    }
  }

  isSaved() {
    const freelancerId = this.freelancer()?.id;
    return freelancerId ? this.savedFreelancerIds().includes(freelancerId) : false;
  }

  toggleSaved() {
    const freelancer = this.freelancer();
    if (!freelancer || !this.canSaveFreelancers()) {
      return;
    }

    const wasSaved = this.isSaved();
    const request$ = wasSaved
      ? this.favoriteFreelancersService.remove(freelancer.id)
      : this.favoriteFreelancersService.add(freelancer.id);

    this.saving.set(true);
    this.errorMessage.set('');

    request$
      .pipe(
        finalize(() => this.saving.set(false)),
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

  openInvite() {
    this.inviteOpen.set(true);
    this.selectedProjectId.set(this.projects()[0]?.id ?? '');
  }

  closeInvite() {
    this.inviteOpen.set(false);
    this.selectedProjectId.set('');
  }

  inviteToJob() {
    const freelancer = this.freelancer();
    const project = this.projects().find((item) => item.id === this.selectedProjectId());

    if (!freelancer || !project) {
      this.errorMessage.set('Select one of your jobs first so you can send the invite.');
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

  messageFreelancer() {
    const freelancer = this.freelancer();
    if (!freelancer) {
      return;
    }

    void this.router.navigate(['/messages'], {
      queryParams: {
        recipientEmail: freelancer.email,
        subject: `Intro with ${freelancer.name}`,
        initialMessage: `Hi ${freelancer.name}, I'd like to learn more about your availability and fit for an upcoming project.`,
      },
    });
  }

  openHireNow() {
    this.hireNowOpen.set(true);
    this.inviteOpen.set(false);
  }

  closeHireNow() {
    this.hireNowOpen.set(false);
  }

  updateHireField(field: keyof HireFreelancerRequest, value: string | number) {
    this.hireForm.update((current) => ({ ...current, [field]: value }));
  }

  submitHireNow() {
    const payload = this.hireForm();
    const freelancer = this.freelancer();

    if (!freelancer || !payload.title.trim() || !payload.description.trim()) {
      this.errorMessage.set('Add a title and description before creating the engagement.');
      return;
    }

    this.hiring.set(true);
    this.errorMessage.set('');

    this.projectsService
      .hire({
        ...payload,
        freelancerId: freelancer.id,
      })
      .pipe(
        finalize(() => this.hiring.set(false)),
        catchError((error) => {
          this.errorMessage.set(error.error?.message ?? 'We could not hire this freelancer right now.');
          return of(null);
        }),
      )
      .subscribe((project) => {
        if (!project) {
          return;
        }

        this.hireNowOpen.set(false);
        void this.router.navigate(['/client-dashboard']);
      });
  }
}
