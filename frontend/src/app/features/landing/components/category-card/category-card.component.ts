import { Component, Input } from '@angular/core';
import { NgSwitch, NgSwitchCase, NgSwitchDefault } from '@angular/common';
import { Category, CategoryIcon } from '../../data/mock-data';

@Component({
  selector: 'app-category-card',
  standalone: true,
  imports: [NgSwitch, NgSwitchCase, NgSwitchDefault],
  templateUrl: './category-card.component.html',
  styleUrls: ['./category-card.component.scss'],
})
export class CategoryCardComponent {
  @Input({ required: true }) category!: Category;

  // Fallback isto kao u Reactu: ako nešto fali -> Code
  get icon(): CategoryIcon {
    return (this.category?.icon ?? 'Code') as CategoryIcon;
  }

  formatCount(n: number): string {
    return (n ?? 0).toLocaleString();
  }
}
