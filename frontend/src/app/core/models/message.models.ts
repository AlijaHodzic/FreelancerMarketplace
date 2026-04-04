import { UserRole } from './auth.models';

export interface ConversationSummary {
  id: string;
  otherUserId: string;
  otherUserName: string;
  otherUserRole: UserRole;
  subject: string;
  lastMessagePreview: string;
  lastMessageAtUtc: string;
}

export interface Message {
  id: string;
  senderId: string;
  senderName: string;
  content: string;
  createdAtUtc: string;
}

export interface ConversationThread {
  id: string;
  otherUserId: string;
  otherUserName: string;
  otherUserRole: UserRole;
  subject: string;
  messages: Message[];
}

export interface StartConversationRequest {
  recipientEmail: string;
  subject: string;
  initialMessage: string;
}

export interface SendMessageRequest {
  content: string;
}
