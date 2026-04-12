import { CurrencyPipe, DatePipe, NgClass, NgFor, NgIf } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { catchError, finalize, of } from 'rxjs';
import { AdminSummary } from '../../../../core/models/admin.models';
import { AdminService } from '../../../../core/services/admin.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [NgIf, NgFor, NgClass, CurrencyPipe, DatePipe],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.scss',
})
export class AdminDashboardComponent implements OnInit {
  private readonly adminService = inject(AdminService);

  readonly summary = signal<AdminSummary | null>(null);
  readonly loading = signal(true);
  readonly errorMessage = signal('');

  readonly userMix = computed(() => {
    const summary = this.summary();
    if (!summary || summary.totalUsers === 0) {
      return [];
    }

    return [
      { label: 'Clients', value: summary.totalClients, width: (summary.totalClients / summary.totalUsers) * 100, color: 'bg-sky-500' },
      { label: 'Freelancers', value: summary.totalFreelancers, width: (summary.totalFreelancers / summary.totalUsers) * 100, color: 'bg-emerald-500' },
      { label: 'Admins', value: summary.totalAdmins, width: (summary.totalAdmins / summary.totalUsers) * 100, color: 'bg-indigo-500' },
    ];
  });

  ngOnInit() {
    this.adminService
      .getSummary()
      .pipe(
        finalize(() => this.loading.set(false)),
        catchError((error) => {
          this.errorMessage.set(error.error?.message ?? 'We could not load admin data right now.');
          return of(null);
        }),
      )
      .subscribe((summary) => this.summary.set(summary));
  }
}
