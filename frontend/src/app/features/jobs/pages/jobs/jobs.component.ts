import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CurrencyPipe, DatePipe, NgFor, NgIf } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { catchError, finalize, of } from 'rxjs';
import { AuthService } from '../../../../core/auth/auth.service';
import { Project } from '../../../../core/models/project.models';
import { BidsService } from '../../../../core/services/bids.service';
import { ProjectsService } from '../../../../core/services/projects.service';

@Component({
  selector: 'app-jobs',
  standalone: true,
  imports: [NgFor, NgIf, DatePipe, CurrencyPipe, ReactiveFormsModule, RouterLink],
  templateUrl: './jobs.component.html',
  styleUrl: './jobs.component.scss',
})
export class JobsComponent implements OnInit {
  private readonly projectsService = inject(ProjectsService);
  private readonly bidsService = inject(BidsService);
  private readonly authService = inject(AuthService);
  private readonly formBuilder = inject(FormBuilder);

  readonly projects = signal<Project[]>([]);
  readonly loading = signal(true);
  readonly submitting = signal(false);
  readonly errorMessage = signal('');
  readonly successMessage = signal('');
  readonly selectedProjectId = signal<string | null>(null);
  readonly isFreelancer = computed(() => this.authService.role() === 'Freelancer');
  readonly isGuest = computed(() => !this.authService.isAuthenticated());

  readonly proposalForm = this.formBuilder.nonNullable.group({
    amount: [0, [Validators.required, Validators.min(1)]],
    message: ['', [Validators.required, Validators.minLength(20)]],
  });

  ngOnInit() {
    this.loadProjects();
  }

  openProposal(projectId: string) {
    this.selectedProjectId.set(projectId);
    this.successMessage.set('');
    this.errorMessage.set('');
    this.proposalForm.reset({ amount: 0, message: '' });
  }

  cancelProposal() {
    this.selectedProjectId.set(null);
    this.errorMessage.set('');
  }

  submitProposal() {
    if (!this.selectedProjectId() || this.proposalForm.invalid) {
      this.proposalForm.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set('');
    this.successMessage.set('');

    const { amount, message } = this.proposalForm.getRawValue();

    this.bidsService
      .create({
        projectId: this.selectedProjectId()!,
        amount,
        message,
      })
      .pipe(
        finalize(() => this.submitting.set(false)),
        catchError((error) => {
          this.errorMessage.set(error.error?.message ?? 'We could not send your proposal right now.');
          return of(null);
        }),
      )
      .subscribe((response) => {
        if (!response) {
          return;
        }

        this.successMessage.set('Your proposal was sent successfully.');
        this.selectedProjectId.set(null);
      });
  }

  private loadProjects() {
    this.loading.set(true);
    this.projectsService
      .getAll()
      .pipe(
        finalize(() => this.loading.set(false)),
        catchError(() => {
          this.errorMessage.set('We could not load jobs right now. Make sure the backend API is running.');
          return of([]);
        }),
      )
      .subscribe((projects) => this.projects.set(projects));
  }
}
