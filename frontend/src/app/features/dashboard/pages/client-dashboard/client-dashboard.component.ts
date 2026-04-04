import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CurrencyPipe, DatePipe, NgClass, NgFor, NgIf } from '@angular/common';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { catchError, finalize, forkJoin, of } from 'rxjs';
import { Bid, Project } from '../../../../core/models/project.models';
import { BidsService } from '../../../../core/services/bids.service';
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
  private readonly projectsService = inject(ProjectsService);
  private readonly bidsService = inject(BidsService);
  private readonly router = inject(Router);
  private readonly reviewsService = inject(ReviewsService);
  private readonly formBuilder = inject(FormBuilder);

  readonly projects = signal<ClientProjectView[]>([]);
  readonly loading = signal(true);
  readonly actionLoadingId = signal<string | null>(null);
  readonly errorMessage = signal('');
  readonly successMessage = signal('');
  readonly expandedProjectId = signal<string | null>(null);
  readonly reviewProjectId = signal<string | null>(null);

  readonly reviewForm = this.formBuilder.nonNullable.group({
    rating: [5, [Validators.required, Validators.min(1), Validators.max(5)]],
    comment: ['', [Validators.required, Validators.minLength(10)]],
  });

  readonly totalProposals = computed(() => this.projects().reduce((sum, project) => sum + project.bids.length, 0));

  ngOnInit() {
    this.loadProjects();
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

    this.projectsService
      .getMine()
      .pipe(
        catchError((error) => {
          this.errorMessage.set(error.error?.message ?? 'We could not load your jobs right now.');
          return of([] as Project[]);
        }),
      )
      .subscribe((projects) => {
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
