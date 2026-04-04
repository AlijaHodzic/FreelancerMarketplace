export interface NotificationItem {
  id: string;
  type: string;
  title: string;
  message: string;
  link: string;
  isRead: boolean;
  createdAtUtc: string;
}

export interface NotificationSummary {
  unreadCount: number;
}
