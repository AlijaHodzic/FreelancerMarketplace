import { CurrencyPipe, DatePipe, NgClass, NgFor, NgIf } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { catchError, finalize, forkJoin, of } from 'rxjs';
import { AdminActivity, AdminProject, AdminSummary, AdminUser } from '../../../../core/models/admin.models';
import { AdminService } from '../../../../core/services/admin.service';

type AdminTab = 'overview' | 'users' | 'projects' | 'activity';
type UserRoleFilter = 'All' | 'Admin' | 'Client' | 'Freelancer';
type ProjectStatusFilter = 'All' | 'Draft' | 'Open' | 'InProgress' | 'Completed' | 'Cancelled';

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
  readonly users = signal<AdminUser[]>([]);
  readonly projects = signal<AdminProject[]>([]);
  readonly activity = signal<AdminActivity[]>([]);
  readonly loading = signal(true);
  readonly errorMessage = signal('');
  readonly activeTab = signal<AdminTab>('overview');
  readonly userRoleFilter = signal<UserRoleFilter>('All');
  readonly projectStatusFilter = signal<ProjectStatusFilter>('All');

  readonly tabs: { key: AdminTab; label: string }[] = [
    { key: 'overview', label: 'Overview' },
    { key: 'users', label: 'Users' },
    { key: 'projects', label: 'Projects' },
    { key: 'activity', label: 'Activity' },
  ];

  readonly userRoleOptions: UserRoleFilter[] = ['All', 'Admin', 'Client', 'Freelancer'];
  readonly projectStatusOptions: ProjectStatusFilter[] = ['All', 'Draft', 'Open', 'InProgress', 'Completed', 'Cancelled'];

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

  readonly filteredUsers = computed(() => {
    const role = this.userRoleFilter();
    const users = this.users();

    if (role === 'All') {
      return users;
    }

    return users.filter((user) => user.role === role);
  });

  readonly filteredProjects = computed(() => {
    const status = this.projectStatusFilter();
    const projects = this.projects();

    if (status === 'All') {
      return projects;
    }

    return projects.filter((project) => project.status === status);
  });

  ngOnInit() {
    forkJoin({
      summary: this.adminService.getSummary(),
      users: this.adminService.getUsers(),
      projects: this.adminService.getProjects(),
      activity: this.adminService.getActivity(),
    })
      .pipe(
        finalize(() => this.loading.set(false)),
        catchError((error) => {
          this.errorMessage.set(error.error?.message ?? 'We could not load admin data right now.');
          return of(null);
        }),
      )
      .subscribe((result) => {
        if (!result) {
          return;
        }

        this.summary.set(result.summary);
        this.users.set(result.users);
        this.projects.set(result.projects);
        this.activity.set(result.activity);
      });
  }
}
