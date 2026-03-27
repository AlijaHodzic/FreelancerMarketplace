import { Component, Input } from '@angular/core';
import { NgFor, NgClass } from '@angular/common';
import { Testimonial } from '../../data/mock-data';

@Component({
  selector: 'app-testimonial-card',
  standalone: true,
  imports: [NgFor, NgClass],
  templateUrl: './testimonial-card.component.html',
  styleUrls: ['./testimonial-card.component.scss'],
})
export class TestimonialCardComponent {
  @Input({ required: true }) testimonial!: Testimonial;

  stars = Array.from({ length: 5 });
}
