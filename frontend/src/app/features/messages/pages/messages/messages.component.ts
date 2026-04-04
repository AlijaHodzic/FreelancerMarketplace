import { DatePipe, NgClass, NgFor, NgIf } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { catchError, finalize, of } from 'rxjs';
import { AuthService } from '../../../../core/auth/auth.service';
import { ConversationSummary, ConversationThread, Message } from '../../../../core/models/message.models';
import { MessagesService } from '../../../../core/services/messages.service';
import { getConversationStatusBadge } from '../../../../core/utils/status-badges';

@Component({
  selector: 'app-messages',
  standalone: true,
  imports: [NgIf, NgFor, NgClass, DatePipe, ReactiveFormsModule],
  templateUrl: './messages.component.html',
  styleUrl: './messages.component.scss',
})
export class MessagesComponent implements OnInit {
  private readonly messagesService = inject(MessagesService);
  private readonly authService = inject(AuthService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly conversations = signal<ConversationSummary[]>([]);
  readonly selectedConversation = signal<ConversationThread | null>(null);
  readonly loadingConversations = signal(true);
  readonly loadingThread = signal(false);
  readonly submitting = signal(false);
  readonly errorMessage = signal('');
  readonly successMessage = signal('');
  readonly composeMode = signal(false);

  readonly currentUser = this.authService.user;
  readonly selectedConversationId = computed(() => this.selectedConversation()?.id ?? null);
  readonly conversationStatusBadge = getConversationStatusBadge;

  readonly composeForm = this.formBuilder.nonNullable.group({
    recipientEmail: ['', [Validators.required, Validators.email]],
    subject: ['', [Validators.required, Validators.minLength(3)]],
    initialMessage: ['', [Validators.required, Validators.minLength(2)]],
  });

  readonly replyForm = this.formBuilder.nonNullable.group({
    content: ['', [Validators.required, Validators.minLength(1)]],
  });

  ngOnInit() {
    this.loadConversations();

    this.route.queryParamMap.subscribe((params) => {
      const conversationId = params.get('conversationId');
      const recipientEmail = params.get('recipientEmail') ?? '';
      const subject = params.get('subject') ?? '';
      const initialMessage = params.get('initialMessage') ?? '';

      if (conversationId) {
        this.composeMode.set(false);
        this.loadConversation(conversationId);
        return;
      }

      if (recipientEmail || subject || initialMessage) {
        this.composeMode.set(true);
        this.selectedConversation.set(null);
        this.composeForm.setValue({
          recipientEmail,
          subject,
          initialMessage,
        });
      }
    });
  }

  isOwnMessage(message: Message) {
    return message.senderId === this.currentUser()?.id;
  }

  openCompose() {
    this.composeMode.set(true);
    this.selectedConversation.set(null);
    this.errorMessage.set('');
    this.successMessage.set('');
    this.composeForm.reset({
      recipientEmail: '',
      subject: '',
      initialMessage: '',
    });
    this.replyForm.reset();
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { conversationId: null, recipientEmail: null, subject: null, initialMessage: null },
      queryParamsHandling: 'merge',
    });
  }

  selectConversation(conversation: ConversationSummary) {
    this.composeMode.set(false);
    this.errorMessage.set('');
    this.successMessage.set('');
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: {
        conversationId: conversation.id,
        recipientEmail: null,
        subject: null,
        initialMessage: null,
      },
      queryParamsHandling: 'merge',
    });
  }

  submitCompose() {
    if (this.composeForm.invalid) {
      this.composeForm.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set('');
    this.successMessage.set('');

    this.messagesService
      .startConversation(this.composeForm.getRawValue())
      .pipe(
        finalize(() => this.submitting.set(false)),
        catchError((error) => {
          this.errorMessage.set(error.error?.message ?? 'We could not start this conversation right now.');
          return of(null);
        }),
      )
      .subscribe((conversation) => {
        if (!conversation) {
          return;
        }

        this.composeMode.set(false);
        this.composeForm.reset({
          recipientEmail: '',
          subject: '',
          initialMessage: '',
        });
        this.replyForm.reset({ content: '' });
        this.successMessage.set('Conversation is ready.');
        this.selectedConversation.set(conversation);
        this.loadConversations(conversation.id);
        void this.router.navigate([], {
          relativeTo: this.route,
          queryParams: {
            conversationId: conversation.id,
            recipientEmail: null,
            subject: null,
            initialMessage: null,
          },
          queryParamsHandling: 'merge',
        });
      });
  }

  sendReply() {
    const conversationId = this.selectedConversationId();
    if (!conversationId || this.replyForm.invalid) {
      this.replyForm.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set('');

    this.messagesService
      .sendMessage(conversationId, this.replyForm.getRawValue())
      .pipe(
        finalize(() => this.submitting.set(false)),
        catchError((error) => {
          this.errorMessage.set(error.error?.message ?? 'We could not send your message right now.');
          return of(null);
        }),
      )
      .subscribe((message) => {
        if (!message) {
          return;
        }

        this.selectedConversation.update((conversation) =>
          conversation
            ? {
                ...conversation,
                messages: [...conversation.messages, message],
              }
            : conversation,
        );
        this.replyForm.reset({ content: '' });
        this.loadConversations(conversationId);
      });
  }

  private loadConversations(preferredConversationId?: string) {
    this.loadingConversations.set(true);

    this.messagesService
      .getConversations()
      .pipe(
        finalize(() => this.loadingConversations.set(false)),
        catchError((error) => {
          this.errorMessage.set(error.error?.message ?? 'We could not load your conversations right now.');
          return of([] as ConversationSummary[]);
        }),
      )
      .subscribe((conversations) => {
        this.conversations.set(conversations);

        const routeConversationId = this.route.snapshot.queryParamMap.get('conversationId');
        const targetConversationId =
          preferredConversationId ?? routeConversationId ?? this.selectedConversationId() ?? conversations[0]?.id ?? null;

        if (!targetConversationId || this.composeMode()) {
          return;
        }

        if (this.selectedConversationId() !== targetConversationId) {
          this.loadConversation(targetConversationId);
        }
      });
  }

  private loadConversation(conversationId: string) {
    this.loadingThread.set(true);
    this.errorMessage.set('');

    this.messagesService
      .getConversation(conversationId)
      .pipe(
        finalize(() => this.loadingThread.set(false)),
        catchError((error) => {
          this.errorMessage.set(error.error?.message ?? 'We could not open this conversation right now.');
          return of(null);
        }),
      )
      .subscribe((conversation) => {
        if (!conversation) {
          return;
        }

        this.selectedConversation.set(conversation);
        this.replyForm.reset({ content: '' });
      });
  }
}
