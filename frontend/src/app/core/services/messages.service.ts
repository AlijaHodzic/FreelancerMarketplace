import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { API_BASE_URL } from '../config/api.config';
import {
  ConversationSummary,
  ConversationThread,
  Message,
  SendMessageRequest,
  StartConversationRequest,
} from '../models/message.models';

@Injectable({ providedIn: 'root' })
export class MessagesService {
  private readonly http = inject(HttpClient);

  getConversations() {
    return this.http.get<ConversationSummary[]>(`${API_BASE_URL}/messages/conversations`);
  }

  getConversation(conversationId: string) {
    return this.http.get<ConversationThread>(`${API_BASE_URL}/messages/conversations/${conversationId}`);
  }

  startConversation(payload: StartConversationRequest) {
    return this.http.post<ConversationThread>(`${API_BASE_URL}/messages/conversations`, payload);
  }

  sendMessage(conversationId: string, payload: SendMessageRequest) {
    return this.http.post<Message>(`${API_BASE_URL}/messages/conversations/${conversationId}/messages`, payload);
  }
}
