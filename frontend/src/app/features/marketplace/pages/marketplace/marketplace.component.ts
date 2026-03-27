import { Component } from '@angular/core';
import { NgFor } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FREELANCERS } from '../../data/freelancers.data';

@Component({
  selector: 'app-marketplace',
  standalone: true,
  imports: [NgFor, RouterLink],
  templateUrl: './marketplace.component.html',
  styleUrl: './marketplace.component.scss',
})
export class MarketplaceComponent {
  readonly filters = ['Web Development', 'Design', 'Writing', 'Marketing', 'Video', 'Data'];

  readonly freelancers = FREELANCERS;
}
