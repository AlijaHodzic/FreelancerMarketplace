import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, map, of, tap } from 'rxjs';
import { API_BASE_URL } from '../config/api.config';
import { NotificationItem, NotificationSummary } from '../models/notification.models';

@Injectable({ providedIn: 'root' })
export class NotificationsService {
  private readonly http = inject(HttpClient);
  private readonly notificationsState = signal<NotificationItem[]>([]);
  private readonly unreadCountState = signal(0);

  readonly notifications = computed(() => this.notificationsState());
  readonly unreadCount = computed(() => this.unreadCountState());

  loadSummary() {
    return this.http.get<NotificationSummary>(`${API_BASE_URL}/notifications/summary`).pipe(
      tap((summary) => this.unreadCountState.set(summary.unreadCount)),
      catchError(() => {
        this.unreadCountState.set(0);
        return of({ unreadCount: 0 } satisfies NotificationSummary);
      }),
    );
  }

  loadAll() {
    return this.http.get<NotificationItem[]>(`${API_BASE_URL}/notifications`).pipe(
      tap((items) => {
        this.notificationsState.set(items);
        this.unreadCountState.set(items.filter((item) => !item.isRead).length);
      }),
      catchError(() => {
        this.notificationsState.set([]);
        this.unreadCountState.set(0);
        return of([] as NotificationItem[]);
      }),
    );
  }

  markAsRead(notificationId: string) {
    return this.http.post<void>(`${API_BASE_URL}/notifications/${notificationId}/read`, {}).pipe(
      tap(() => {
        this.notificationsState.update((items) =>
          items.map((item) => (item.id === notificationId ? { ...item, isRead: true } : item)),
        );
        this.unreadCountState.set(this.notificationsState().filter((item) => !item.isRead).length);
      }),
      map(() => void 0),
    );
  }

  markAllAsRead() {
    return this.http.post<void>(`${API_BASE_URL}/notifications/read-all`, {}).pipe(
      tap(() => {
        this.notificationsState.update((items) => items.map((item) => ({ ...item, isRead: true })));
        this.unreadCountState.set(0);
      }),
      map(() => void 0),
    );
  }

  clear() {
    this.notificationsState.set([]);
    this.unreadCountState.set(0);
  }
}
