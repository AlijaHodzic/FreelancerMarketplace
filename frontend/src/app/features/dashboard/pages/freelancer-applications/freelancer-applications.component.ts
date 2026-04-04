import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CurrencyPipe, DatePipe, NgClass, NgFor, NgIf } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { catchError, finalize, of } from 'rxjs';
import { Bid } from '../../../../core/models/project.models';
import { BidsService } from '../../../../core/services/bids.service';
import { getBidStatusBadge } from '../../../../core/utils/status-badges';

type ApplicationFilter = 'All' | 'Pending' | 'Accepted' | 'Rejected';

@Component({
  selector: 'app-freelancer-applications',
  standalone: true,
  imports: [NgIf, NgFor, NgClass, CurrencyPipe, DatePipe, FormsModule],
  templateUrl: './freelancer-applications.component.html',
  styleUrl: './freelancer-applications.component.scss',
})
export class FreelancerApplicationsComponent implements OnInit {
  private readonly bidsService = inject(BidsService);
  private readonly router = inject(Router);

  readonly bids = signal<Bid[]>([]);
  readonly loading = signal(true);
  readonly errorMessage = signal('');
  readonly selectedFilter = signal<ApplicationFilter>('All');
  readonly searchTerm = signal('');
  readonly sortBy = signal<'latest' | 'highest-offer' | 'lowest-offer'>('latest');

  readonly filters: ApplicationFilter[] = ['All', 'Pending', 'Accepted', 'Rejected'];
  readonly bidStatusBadge = getBidStatusBadge;

  readonly filteredBids = computed(() => {
    const filter = this.selectedFilter();
    const query = this.searchTerm().trim().toLowerCase();
    const sortBy = this.sortBy();
    let bids = this.bids();

    if (filter !== 'All') {
      bids = bids.filter((bid) => bid.status === filter);
    }

    if (query) {
      bids = bids.filter((bid) =>
        [bid.projectTitle, bid.projectDescription, bid.clientName, bid.message].join(' ').toLowerCase().includes(query),
      );
    }

    return [...bids].sort((left, right) => {
      switch (sortBy) {
        case 'highest-offer':
          return right.amount - left.amount;
        case 'lowest-offer':
          return left.amount - right.amount;
        default:
          return new Date(right.createdAtUtc).getTime() - new Date(left.createdAtUtc).getTime();
      }
    });
  });

  ngOnInit() {
    this.loadApplications();
  }

  setFilter(filter: ApplicationFilter) {
    this.selectedFilter.set(filter);
  }

  messageClient(bid: Bid) {
    void this.router.navigate(['/messages'], {
      queryParams: {
        recipientEmail: bid.clientEmail,
        subject: `${bid.projectTitle} follow-up`,
        initialMessage: `Hi ${bid.clientName}, I'm following up on my proposal for "${bid.projectTitle}".`,
      },
    });
  }

  private loadApplications() {
    this.loading.set(true);
    this.errorMessage.set('');

    this.bidsService
      .getMine()
      .pipe(
        finalize(() => this.loading.set(false)),
        catchError((error) => {
          this.errorMessage.set(error.error?.message ?? 'We could not load your applications right now.');
          return of([]);
        }),
      )
      .subscribe((bids) => this.bids.set(bids));
  }
}
