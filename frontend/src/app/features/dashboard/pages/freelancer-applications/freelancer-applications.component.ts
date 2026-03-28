import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CurrencyPipe, DatePipe, NgClass, NgFor, NgIf } from '@angular/common';
import { catchError, finalize, of } from 'rxjs';
import { Bid } from '../../../../core/models/project.models';
import { BidsService } from '../../../../core/services/bids.service';

type ApplicationFilter = 'All' | 'Pending' | 'Accepted' | 'Rejected';

@Component({
  selector: 'app-freelancer-applications',
  standalone: true,
  imports: [NgIf, NgFor, NgClass, CurrencyPipe, DatePipe],
  templateUrl: './freelancer-applications.component.html',
  styleUrl: './freelancer-applications.component.scss',
})
export class FreelancerApplicationsComponent implements OnInit {
  private readonly bidsService = inject(BidsService);

  readonly bids = signal<Bid[]>([]);
  readonly loading = signal(true);
  readonly errorMessage = signal('');
  readonly selectedFilter = signal<ApplicationFilter>('All');

  readonly filters: ApplicationFilter[] = ['All', 'Pending', 'Accepted', 'Rejected'];

  readonly filteredBids = computed(() => {
    const filter = this.selectedFilter();
    const bids = this.bids();

    if (filter === 'All') {
      return bids;
    }

    return bids.filter((bid) => bid.status === filter);
  });

  ngOnInit() {
    this.loadApplications();
  }

  setFilter(filter: ApplicationFilter) {
    this.selectedFilter.set(filter);
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
