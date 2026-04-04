import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe, NgClass, NgFor, NgIf } from '@angular/common';
import { Router } from '@angular/router';
import { catchError, finalize, of } from 'rxjs';
import { NotificationItem } from '../../../../core/models/notification.models';
import { NotificationsService } from '../../../../core/services/notifications.service';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [NgIf, NgFor, NgClass, DatePipe],
  templateUrl: './notifications.component.html',
  styleUrl: './notifications.component.scss',
})
export class NotificationsComponent implements OnInit {
  private readonly notificationsService = inject(NotificationsService);
  private readonly router = inject(Router);

  readonly notifications = signal<NotificationItem[]>([]);
  readonly loading = signal(true);
  readonly errorMessage = signal('');

  ngOnInit() {
    this.loadNotifications();
  }

  openNotification(notification: NotificationItem) {
    const request$ = notification.isRead ? of(void 0) : this.notificationsService.markAsRead(notification.id);

    request$.subscribe(() => {
      this.notifications.update((items) => items.map((item) => (item.id === notification.id ? { ...item, isRead: true } : item)));

      if (notification.link) {
        void this.router.navigateByUrl(notification.link);
      }
    });
  }

  markAllAsRead() {
    this.notificationsService.markAllAsRead().subscribe(() => {
      this.notifications.update((items) => items.map((item) => ({ ...item, isRead: true })));
    });
  }

  private loadNotifications() {
    this.loading.set(true);
    this.errorMessage.set('');

    this.notificationsService
      .loadAll()
      .pipe(
        finalize(() => this.loading.set(false)),
        catchError((error) => {
          this.errorMessage.set(error.error?.message ?? 'We could not load your notifications right now.');
          return of([] as NotificationItem[]);
        }),
      )
      .subscribe((items) => this.notifications.set(items));
  }
}
