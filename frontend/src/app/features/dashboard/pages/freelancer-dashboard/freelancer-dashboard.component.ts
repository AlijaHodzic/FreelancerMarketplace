import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CurrencyPipe, NgClass, NgFor, NgIf } from '@angular/common';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { catchError, finalize, forkJoin, of } from 'rxjs';
import { AuthService } from '../../../../core/auth/auth.service';
import { FreelancerPortfolioItem, FreelancerProfile } from '../../../../core/models/freelancer.models';
import { Bid } from '../../../../core/models/project.models';
import { BidsService } from '../../../../core/services/bids.service';
import { FreelancersService } from '../../../../core/services/freelancers.service';
import { NotificationsService } from '../../../../core/services/notifications.service';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-freelancer-dashboard',
  standalone: true,
  imports: [NgIf, NgFor, NgClass, CurrencyPipe, FormsModule, ReactiveFormsModule, RouterLink],
  templateUrl: './freelancer-dashboard.component.html',
  styleUrl: './freelancer-dashboard.component.scss',
})
export class FreelancerDashboardComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly freelancersService = inject(FreelancersService);
  private readonly bidsService = inject(BidsService);
  private readonly notificationsService = inject(NotificationsService);
  private readonly formBuilder = inject(FormBuilder);

  readonly user = this.authService.user;
  readonly profile = signal<FreelancerProfile | null>(null);
  readonly bids = signal<Bid[]>([]);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly payoutSubmitting = signal(false);
  readonly editOpen = signal(false);
  readonly payoutOpen = signal(false);
  readonly errorMessage = signal('');
  readonly successMessage = signal('');
  readonly avatarPreview = signal('');

  readonly profileForm = this.formBuilder.nonNullable.group({
    fullName: ['', [Validators.required, Validators.minLength(2)]],
    headline: ['', [Validators.required, Validators.minLength(3)]],
    avatar: ['', [Validators.required, Validators.minLength(8)]],
    location: ['', [Validators.required, Validators.minLength(2)]],
    category: ['', [Validators.required, Validators.minLength(2)]],
    experience: ['', [Validators.required, Validators.minLength(2)]],
    responseTime: ['', [Validators.required, Validators.minLength(2)]],
    successRate: ['', [Validators.required, Validators.minLength(2)]],
    completedProjects: [0, [Validators.required, Validators.min(0)]],
    hourlyRate: [25, [Validators.required, Validators.min(1)]],
    description: ['', [Validators.required, Validators.minLength(12)]],
    about: ['', [Validators.required, Validators.minLength(24)]],
    skillsText: ['', [Validators.required, Validators.minLength(2)]],
  });

  readonly payoutForm = this.formBuilder.nonNullable.group({
    provider: ['Payoneer', [Validators.required]],
    amount: [250, [Validators.required, Validators.min(1)]],
  });

  readonly portfolioItems = signal<FreelancerPortfolioItem[]>([]);
  readonly activeContracts = computed(() => this.bids().filter((bid) => bid.status === 'Accepted'));
  readonly pendingApplications = computed(() => this.bids().filter((bid) => bid.status === 'Pending').length);
  readonly profileCompletion = computed(() => {
    const profile = this.profile();
    if (!profile) {
      return 0;
    }

    const checks = [
      profile.avatar,
      profile.title,
      profile.location,
      profile.category,
      profile.description,
      profile.about,
      profile.skills.length > 0 ? 'yes' : '',
      profile.portfolio.length > 0 ? 'yes' : '',
    ];

    return Math.round((checks.filter(Boolean).length / checks.length) * 100);
  });

  ngOnInit() {
    this.loadDashboard();
  }

  openEdit() {
    const profile = this.profile();
    if (!profile) {
      return;
    }

    this.profileForm.reset({
      fullName: profile.name,
      headline: profile.title,
      avatar: profile.avatar,
      location: profile.location,
      category: profile.category,
      experience: profile.experience,
      responseTime: profile.responseTime,
      successRate: profile.successRate,
      completedProjects: profile.completedProjects,
      hourlyRate: profile.hourlyRate,
      description: profile.description,
      about: profile.about,
      skillsText: profile.skills.join(', '),
    });
    this.avatarPreview.set(profile.avatar);
    this.portfolioItems.set(profile.portfolio.map((item) => ({ ...item, tags: [...item.tags] })));
    this.editOpen.set(true);
    this.errorMessage.set('');
    this.successMessage.set('');
  }

  closeEdit() {
    this.editOpen.set(false);
  }

  openPayout() {
    const profile = this.profile();
    this.payoutForm.reset({
      provider: 'Payoneer',
      amount: profile ? Math.max(Math.round(profile.hourlyRate * 10), 50) : 250,
    });
    this.payoutOpen.set(true);
    this.errorMessage.set('');
    this.successMessage.set('');
  }

  closePayout() {
    this.payoutOpen.set(false);
  }

  submitPayout() {
    if (this.payoutForm.invalid) {
      this.payoutForm.markAllAsTouched();
      return;
    }

    this.payoutSubmitting.set(true);
    this.errorMessage.set('');

    const { provider, amount } = this.payoutForm.getRawValue();

    setTimeout(() => {
      this.payoutSubmitting.set(false);
      this.payoutOpen.set(false);
      this.successMessage.set(`Demo payout request submitted via ${provider} for ${amount.toLocaleString('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 })}.`);
    }, 700);
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
      this.errorMessage.set('Please choose a valid image file for your profile photo.');
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

  addPortfolioItem() {
    this.portfolioItems.update((items) => [...items, { title: '', summary: '', image: '', tags: [] }]);
  }

  removePortfolioItem(index: number) {
    this.portfolioItems.update((items) => items.filter((_, itemIndex) => itemIndex !== index));
  }

  updatePortfolioField(index: number, field: keyof FreelancerPortfolioItem, value: string) {
    this.portfolioItems.update((items) =>
      items.map((item, itemIndex) =>
        itemIndex === index
          ? {
              ...item,
              [field]:
                field === 'tags'
                  ? value
                      .split(',')
                      .map((tag) => tag.trim())
                      .filter(Boolean)
                  : value,
            }
          : item,
      ),
    );
  }

  saveProfile() {
    if (this.profileForm.invalid) {
      this.profileForm.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.errorMessage.set('');
    this.successMessage.set('');

    const formValue = this.profileForm.getRawValue();
    this.freelancersService
      .updateMineProfile({
        fullName: formValue.fullName.trim(),
        headline: formValue.headline.trim(),
        avatar: formValue.avatar.trim(),
        location: formValue.location.trim(),
        category: formValue.category.trim(),
        experience: formValue.experience.trim(),
        responseTime: formValue.responseTime.trim(),
        successRate: formValue.successRate.trim(),
        completedProjects: formValue.completedProjects,
        hourlyRate: formValue.hourlyRate,
        description: formValue.description.trim(),
        about: formValue.about.trim(),
        skills: formValue.skillsText
          .split(',')
          .map((skill) => skill.trim())
          .filter(Boolean),
        portfolio: this.portfolioItems(),
      })
      .pipe(
        finalize(() => this.saving.set(false)),
        catchError((error) => {
          this.errorMessage.set(error.error?.message ?? 'We could not save your profile right now.');
          return of(null);
        }),
      )
      .subscribe((profile) => {
        if (!profile) {
          return;
        }

        this.profile.set(profile);
        this.authService.updateCurrentUser({ fullName: profile.name });
        this.editOpen.set(false);
        this.successMessage.set('Profile updated successfully.');
      });
  }

  private loadDashboard() {
    this.loading.set(true);
    this.errorMessage.set('');

    forkJoin({
      profile: this.freelancersService.getMineProfile().pipe(catchError(() => of(null))),
      bids: this.bidsService.getMine().pipe(catchError(() => of([] as Bid[]))),
      notifications: this.notificationsService.loadSummary(),
    })
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe(({ profile, bids }) => {
        if (!profile) {
          this.errorMessage.set('We could not load your freelancer workspace right now.');
          return;
        }

        this.profile.set(profile);
        this.bids.set(bids);
      });
  }
}
