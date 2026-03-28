import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CurrencyPipe, DatePipe, NgClass, NgFor, NgIf } from '@angular/common';
import { catchError, finalize, forkJoin, of } from 'rxjs';
import { Bid, Project } from '../../../../core/models/project.models';
import { BidsService } from '../../../../core/services/bids.service';
import { ProjectsService } from '../../../../core/services/projects.service';

interface ClientProjectView extends Project {
  bids: Bid[];
}

@Component({
  selector: 'app-client-dashboard',
  standalone: true,
  imports: [NgIf, NgFor, NgClass, CurrencyPipe, DatePipe],
  templateUrl: './client-dashboard.component.html',
  styleUrl: './client-dashboard.component.scss',
})
export class ClientDashboardComponent implements OnInit {
  private readonly projectsService = inject(ProjectsService);
  private readonly bidsService = inject(BidsService);

  readonly projects = signal<ClientProjectView[]>([]);
  readonly loading = signal(true);
  readonly actionLoadingId = signal<string | null>(null);
  readonly errorMessage = signal('');
  readonly successMessage = signal('');
  readonly expandedProjectId = signal<string | null>(null);

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

            return {
              ...project,
              status: action === 'accept' ? 'InProgress' : project.status,
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
