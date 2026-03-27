import { Component, inject, signal } from '@angular/core';
import { NgClass, NgFor, NgIf } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { catchError, finalize, of } from 'rxjs';
import { ProjectsService } from '../../../../core/services/projects.service';

@Component({
  selector: 'app-post-job',
  standalone: true,
  imports: [NgFor, NgIf, NgClass, RouterLink, ReactiveFormsModule],
  templateUrl: './post-job.component.html',
  styleUrl: './post-job.component.scss',
})
export class PostJobComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly projectsService = inject(ProjectsService);

  readonly loading = signal(false);
  readonly successMessage = signal('');
  readonly errorMessage = signal('');
  readonly checklist = [
    'What outcome do you need delivered?',
    'What skills or tools are required?',
    'What budget range works for you?',
    'What is your target delivery date?',
  ] as const;

  readonly form = this.formBuilder.nonNullable.group({
    title: ['', [Validators.required, Validators.minLength(5)]],
    category: ['Web Development', Validators.required],
    description: ['', [Validators.required, Validators.minLength(30)]],
    budgetType: ['fixed', Validators.required],
    budgetAmount: [5000, [Validators.required, Validators.min(1)]],
    deadline: ['', Validators.required],
  });

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.successMessage.set('');
    this.errorMessage.set('');

    const { title, description, budgetAmount } = this.form.getRawValue();

    this.projectsService
      .create({
        title,
        description,
        budgetMin: budgetAmount,
        budgetMax: budgetAmount,
      })
      .pipe(
        finalize(() => this.loading.set(false)),
        catchError((error) => {
          this.errorMessage.set(error.error?.message ?? 'We could not post this job. Make sure the backend is running and you are signed in as a client.');
          return of(null);
        }),
      )
      .subscribe((project) => {
        if (!project) {
          return;
        }

        this.successMessage.set('Your job was posted successfully.');
        this.form.reset({
          title: '',
          category: 'Web Development',
          description: '',
          budgetType: 'fixed',
          budgetAmount: 5000,
          deadline: '',
        });
      });
  }
}
