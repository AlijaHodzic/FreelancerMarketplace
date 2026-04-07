import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CurrencyPipe, DatePipe, NgClass, NgFor, NgIf } from '@angular/common';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { catchError, finalize, forkJoin, of } from 'rxjs';
import { AuthService } from '../../../../core/auth/auth.service';
import { ClientProfile } from '../../../../core/models/client-profile.models';
import { Bid, Project } from '../../../../core/models/project.models';
import { BidsService } from '../../../../core/services/bids.service';
import { ClientProfileService } from '../../../../core/services/client-profile.service';
import { ProjectsService } from '../../../../core/services/projects.service';
import { ReviewsService } from '../../../../core/services/reviews.service';

interface ClientProjectView extends Project {
  bids: Bid[];
}

@Component({
  selector: 'app-client-dashboard',
  standalone: true,
  imports: [NgIf, NgFor, NgClass, CurrencyPipe, DatePipe, FormsModule, ReactiveFormsModule],
  templateUrl: './client-dashboard.component.html',
  styleUrl: './client-dashboard.component.scss',
})
export class ClientDashboardComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly clientProfileService = inject(ClientProfileService);
  private readonly projectsService = inject(ProjectsService);
  private readonly bidsService = inject(BidsService);
  private readonly router = inject(Router);
  private readonly reviewsService = inject(ReviewsService);
  private readonly formBuilder = inject(FormBuilder);

  readonly user = this.authService.user;
  readonly profile = signal<ClientProfile | null>(null);
  readonly projects = signal<ClientProjectView[]>([]);
  readonly loading = signal(true);
  readonly savingProfile = signal(false);
  readonly actionLoadingId = signal<string | null>(null);
  readonly errorMessage = signal('');
  readonly successMessage = signal('');
  readonly expandedProjectId = signal<string | null>(null);
  readonly reviewProjectId = signal<string | null>(null);
  readonly editProfileOpen = signal(false);
  readonly avatarPreview = signal('');

  readonly profileForm = this.formBuilder.nonNullable.group({
    fullName: ['', [Validators.required, Validators.minLength(2)]],
    headline: ['', [Validators.required, Validators.minLength(3)]],
    avatar: ['', [Validators.required, Validators.minLength(8)]],
    location: ['', [Validators.required, Validators.minLength(2)]],
    companyName: ['', [Validators.required, Validators.minLength(2)]],
    companyDescription: ['', [Validators.required, Validators.minLength(12)]],
    about: ['', [Validators.required, Validators.minLength(20)]],
  });

  readonly reviewForm = this.formBuilder.nonNullable.group({
    rating: [5, [Validators.required, Validators.min(1), Validators.max(5)]],
    comment: ['', [Validators.required, Validators.minLength(10)]],
  });

  readonly totalProposals = computed(() => this.projects().reduce((sum, project) => sum + project.bids.length, 0));

  ngOnInit() {
    this.loadProjects();
  }

  openProfileEdit() {
    const profile = this.profile();
    if (!profile) {
      return;
    }

    this.profileForm.reset({
      fullName: profile.fullName,
      headline: profile.headline,
      avatar: profile.avatar,
      location: profile.location,
      companyName: profile.companyName,
      companyDescription: profile.companyDescription,
      about: profile.about,
    });
    this.avatarPreview.set(profile.avatar);
    this.editProfileOpen.set(true);
    this.errorMessage.set('');
    this.successMessage.set('');
  }

  closeProfileEdit() {
    this.editProfileOpen.set(false);
  }

  onAvatarInputChange(value: string) {
    this.avatarPreview.set(value.trim());
  }

  onAvatarFileSelected(event: Event) {
    const input = event.target as HTMLInputElement | null;
    const file = input?.files?.[0];

    if (!file) {
      return;
    }

    if (!file.type.startsWith('image/')) {
      this.errorMessage.set('Please choose a valid image file for your client profile photo.');
      input.value = '';
      return;
    }

    const maxSizeInBytes = 2 * 1024 * 1024;
    if (file.size > maxSizeInBytes) {
      this.errorMessage.set('Profile photo must be smaller than 2 MB.');
      input.value = '';
      return;
    }

    const reader = new FileReader();
    reader.onload = () => {
      const result = typeof reader.result === 'string' ? reader.result : '';
      if (!result) {
        this.errorMessage.set('We could not read that image file. Try a different one.');
        return;
      }

      this.profileForm.controls.avatar.setValue(result);
      this.avatarPreview.set(result);
      this.errorMessage.set('');
    };
    reader.onerror = () => {
      this.errorMessage.set('We could not read that image file. Try a different one.');
    };
    reader.readAsDataURL(file);
  }

  clearAvatar() {
    this.profileForm.controls.avatar.setValue('');
    this.avatarPreview.set('');
  }

  saveProfile() {
    if (this.profileForm.invalid) {
      this.profileForm.markAllAsTouched();
      return;
    }

    this.savingProfile.set(true);
    this.errorMessage.set('');
    this.successMessage.set('');

    this.clientProfileService
      .updateMineProfile({
        ...this.profileForm.getRawValue(),
        fullName: this.profileForm.controls.fullName.value.trim(),
        headline: this.profileForm.controls.headline.value.trim(),
        avatar: this.profileForm.controls.avatar.value.trim(),
        location: this.profileForm.controls.location.value.trim(),
        companyName: this.profileForm.controls.companyName.value.trim(),
        companyDescription: this.profileForm.controls.companyDescription.value.trim(),
        about: this.profileForm.controls.about.value.trim(),
      })
      .pipe(
        finalize(() => this.savingProfile.set(false)),
        catchError((error) => {
          this.errorMessage.set(error.error?.message ?? 'We could not save your client profile right now.');
          return of(null);
        }),
      )
      .subscribe((profile) => {
        if (!profile) {
          return;
        }

        this.profile.set(profile);
        this.authService.updateCurrentUser({ fullName: profile.fullName });
        this.editProfileOpen.set(false);
        this.successMessage.set('Client profile updated successfully.');
      });
  }

  toggleProject(projectId: string) {
    this.expandedProjectId.update((current) => (current === projectId ? null : projectId));
  }

  acceptBid(projectId: string, bidId: string) {
    this.handleBidAction(projectId, bidId, 'accept');
  }

  rejectBid(projectId: string, bidId: string) {
    this.handleBidAction(projectId, bidId, 'reject');
  }

  messageFreelancer(bid: Bid, project: Project) {
    void this.router.navigate(['/messages'], {
      queryParams: {
        recipientEmail: bid.freelancerEmail,
        subject: `${project.title} proposal`,
        initialMessage: `Hi ${bid.freelancerName}, I'd like to discuss your proposal for "${project.title}".`,
      },
    });
  }

  markCompleted(projectId: string) {
    this.actionLoadingId.set(projectId);
    this.errorMessage.set('');
    this.successMessage.set('');

    this.projectsService
      .complete(projectId)
      .pipe(
        finalize(() => this.actionLoadingId.set(null)),
        catchError((error) => {
          this.errorMessage.set(error.error?.message ?? 'We could not complete this project right now.');
          return of(null);
        }),
      )
      .subscribe((result) => {
        if (!result) {
          return;
        }

        this.successMessage.set('Project marked as completed.');
        this.projects.update((projects) => projects.map((project) => (project.id === projectId ? { ...project, ...result } : project)));
      });
  }

  openReview(projectId: string) {
    this.reviewProjectId.set(projectId);
    this.reviewForm.reset({
      rating: 5,
      comment: '',
    });
  }

  closeReview() {
    this.reviewProjectId.set(null);
  }

  submitReview(project: ClientProjectView) {
    if (this.reviewForm.invalid) {
      this.reviewForm.markAllAsTouched();
      return;
    }

    this.actionLoadingId.set(project.id);
    this.errorMessage.set('');
    this.successMessage.set('');

    this.reviewsService
      .create({
        projectId: project.id,
        ...this.reviewForm.getRawValue(),
      })
      .pipe(
        finalize(() => this.actionLoadingId.set(null)),
        catchError((error) => {
          this.errorMessage.set(error.error?.message ?? 'We could not save this review right now.');
          return of(null);
        }),
      )
      .subscribe((review) => {
        if (!review) {
          return;
        }

        this.successMessage.set('Review published successfully.');
        this.reviewProjectId.set(null);
        this.projects.update((projects) =>
          projects.map((item) => (item.id === project.id ? { ...item, hasReview: true, canReview: false } : item)),
        );
      });
  }

  private loadProjects() {
    this.loading.set(true);
    this.errorMessage.set('');

    forkJoin({
      profile: this.clientProfileService.getMineProfile().pipe(catchError(() => of(null))),
      projects: this.projectsService.getMine().pipe(
        catchError((error) => {
          this.errorMessage.set(error.error?.message ?? 'We could not load your jobs right now.');
          return of([] as Project[]);
        }),
      ),
    }).subscribe(({ profile, projects }) => {
      this.profile.set(profile);

      if (!projects.length) {
        this.projects.set([]);
        this.loading.set(false);
        return;
      }

      forkJoin(
        projects.map((project) =>
          this.bidsService.getByProject(project.id).pipe(
            catchError(() => of([] as Bid[])),
          ),
        ),
      )
        .pipe(finalize(() => this.loading.set(false)))
        .subscribe((projectBids) => {
          this.projects.set(projects.map((project, index) => ({ ...project, bids: projectBids[index] })));
        });
    });
  }

  private handleBidAction(projectId: string, bidId: string, action: 'accept' | 'reject') {
    this.actionLoadingId.set(bidId);
    this.errorMessage.set('');
    this.successMessage.set('');

    const request$ = action === 'accept' ? this.bidsService.accept(bidId) : this.bidsService.reject(bidId);

    request$
      .pipe(
        finalize(() => this.actionLoadingId.set(null)),
        catchError((error) => {
          this.errorMessage.set(error.error?.message ?? `We could not ${action} this proposal right now.`);
          return of(null);
        }),
      )
      .subscribe((result) => {
        if (result === null) {
          return;
        }

        this.successMessage.set(action === 'accept' ? 'Proposal accepted successfully.' : 'Proposal rejected successfully.');

        this.projects.update((projects) =>
          projects.map((project) => {
            if (project.id !== projectId) {
              return project;
            }

            const acceptedBid = project.bids.find((bid) => bid.id === bidId);

            return {
              ...project,
              status: action === 'accept' ? 'InProgress' : project.status,
              assignedFreelancerId: action === 'accept' ? acceptedBid?.freelancerId ?? project.assignedFreelancerId : project.assignedFreelancerId,
              assignedFreelancerName:
                action === 'accept'
                  ? acceptedBid?.freelancerName ?? project.assignedFreelancerName
                  : project.assignedFreelancerName,
              assignedFreelancerEmail:
                action === 'accept'
                  ? acceptedBid?.freelancerEmail ?? project.assignedFreelancerEmail
                  : project.assignedFreelancerEmail,
              bids: project.bids.map((bid) => {
                if (bid.id === bidId) {
                  return { ...bid, status: action === 'accept' ? 'Accepted' : 'Rejected' };
                }

                if (action === 'accept') {
                  return { ...bid, status: 'Rejected' };
                }

                return bid;
              }),
            };
          }),
        );
      });
  }
}
