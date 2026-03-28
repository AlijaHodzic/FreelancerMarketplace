import { Component, computed, inject } from '@angular/core';
import { CurrencyPipe, NgClass, NgFor, NgIf } from '@angular/common';
import { AuthService } from '../../../../core/auth/auth.service';

interface EarningsPoint {
  month: string;
  amount: number;
}

interface ContractSummary {
  client: string;
  project: string;
  status: string;
  amount: number;
  deliveredAt: string;
}

interface PayoutMethod {
  provider: string;
  email: string;
  status: string;
  accentClass: string;
}

@Component({
  selector: 'app-freelancer-dashboard',
  standalone: true,
  imports: [NgIf, NgFor, NgClass, CurrencyPipe],
  templateUrl: './freelancer-dashboard.component.html',
  styleUrl: './freelancer-dashboard.component.scss',
})
export class FreelancerDashboardComponent {
  private readonly authService = inject(AuthService);

  readonly user = this.authService.user;

  readonly earnings: EarningsPoint[] = [
    { month: 'Oct', amount: 2100 },
    { month: 'Nov', amount: 2900 },
    { month: 'Dec', amount: 3600 },
    { month: 'Jan', amount: 4100 },
    { month: 'Feb', amount: 3900 },
    { month: 'Mar', amount: 4700 },
  ];

  readonly activeContracts: ContractSummary[] = [
    {
      client: 'Nova Commerce',
      project: 'Homepage refresh and conversion cleanup',
      status: 'In review',
      amount: 1500,
      deliveredAt: '2 days ago',
    },
    {
      client: 'Metric Labs',
      project: 'Analytics dashboard revamp',
      status: 'Milestone funded',
      amount: 2200,
      deliveredAt: 'This week',
    },
    {
      client: 'Atlas Mobile',
      project: 'Client portal integration',
      status: 'Completed',
      amount: 980,
      deliveredAt: 'Last week',
    },
  ];

  readonly payoutMethods: PayoutMethod[] = [
    {
      provider: 'Payoneer',
      email: 'freelancer@payoneer.com',
      status: 'Primary',
      accentClass: 'bg-orange-50 text-orange-600 border-orange-100',
    },
    {
      provider: 'PayPal',
      email: 'freelancer@paypal.com',
      status: 'Connected',
      accentClass: 'bg-sky-50 text-sky-600 border-sky-100',
    },
  ];

  readonly recentTransactions = [
    { label: 'Funds cleared from Nova Commerce', amount: 1500, meta: 'Available for withdrawal' },
    { label: 'Processing fee', amount: -45, meta: 'Marketplace fee' },
    { label: 'Withdrawal to Payoneer', amount: -1200, meta: 'Completed yesterday' },
    { label: 'Milestone funded by Metric Labs', amount: 2200, meta: 'Pending release' },
  ];

  readonly totalEarned = this.earnings.reduce((sum, item) => sum + item.amount, 0);
  readonly availableForWithdrawal = 2850;
  readonly pendingClearance = 2200;
  readonly thisMonthGrowth = '+18%';
  readonly maxBarValue = Math.max(...this.earnings.map((item) => item.amount));

  readonly averageMonthlyRevenue = computed(() => Math.round(this.totalEarned / this.earnings.length));

  barHeight(amount: number) {
    return `${Math.max((amount / this.maxBarValue) * 100, 14)}%`;
  }
}
