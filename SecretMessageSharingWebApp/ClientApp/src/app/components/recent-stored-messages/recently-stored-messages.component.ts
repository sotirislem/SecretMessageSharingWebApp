import { httpResource } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, computed, inject, signal, TemplateRef, viewChild } from '@angular/core';
import { firstValueFrom } from 'rxjs';

import { RecentlyStoredSecretMessage, RecentlyStoredSecretMessagesResponse } from '../../models/api/recently-stored-secret-messages-response.model';
import { SecretMessageDeliveryNotification } from '../../models/api/secret-message-delivery-notification.model';
import { ApiClientService } from '../../services/api-client.service';
import { ModalService } from '../../services/modal.service';
import { ToastService } from '../../services/toast.service';
import { MessageDeliveryDetailsModalComponent } from '../modals/message-delivery-details-modal/message-delivery-details-modal.component';
import { DateTimeFullPipe } from '../../pipes/date-time-full.pipe';

@Component({
	templateUrl: './recently-stored-messages.component.html',
	styleUrl: './recently-stored-messages.component.css',
	changeDetection: ChangeDetectionStrategy.OnPush,
	imports: [DateTimeFullPipe]
})
export class RecentlyStoredMessagesComponent {
	private readonly apiClient = inject(ApiClientService);
	private readonly modalService = inject(ModalService);
	private readonly toastService = inject(ToastService);

	private readonly messagesResource = httpResource<RecentlyStoredSecretMessagesResponse>(
		() => this.apiClient.getRecentlyStoredSecretMessagesUrl());

	private readonly deleteConfirmationBody = viewChild.required<TemplateRef<unknown>>('deleteConfirmationModalBody');

	readonly messagePendingDeletion = signal<RecentlyStoredSecretMessage | null>(null);
	readonly loading = this.messagesResource.isLoading;
	readonly loadError = this.messagesResource.error;
	readonly recentlyStoredSecretMessages = computed(() =>
		this.messagesResource.value()?.recentlyStoredSecretMessages ?? []);
	readonly deliveredCount = computed(() =>
		this.recentlyStoredSecretMessages().filter(msg => msg.deliveryDetails).length);
	readonly pendingCount = computed(() =>
		this.recentlyStoredSecretMessages().length - this.deliveredCount());

	async copyMessageId(id: string): Promise<void> {
		try {
			await navigator.clipboard.writeText(id);
			this.toastService.success('Message ID copied to clipboard.', 'Copied');
		} catch {
			this.toastService.error(
				'Could not copy the message ID. Select and copy it manually.',
				'Copy failed');
		}
	}

	showDeliveryDetails(msg: RecentlyStoredSecretMessage): void {
		const messageDeliveryDetails = <SecretMessageDeliveryNotification>{
			messageId: msg.id,
			messageCreatedOn: msg.createdDateTime,
			messageDeliveredOn: msg.deliveryDetails!.deliveredAt,
			recipientIP: msg.deliveryDetails!.recipientIP,
			recipientClientInfo: msg.deliveryDetails!.recipientClientInfo
		};

		void this.modalService.openModal(
			MessageDeliveryDetailsModalComponent,
			{ isNotification: false, messageDeliveryDetails }
		).catch(() => {
			// The details can be dismissed with Escape or a backdrop click.
		});
	}

	async deleteStoredMessage(msg: RecentlyStoredSecretMessage): Promise<void> {
		this.messagePendingDeletion.set(msg);

		let confirmed: boolean;
		try {
			confirmed = await this.modalService.openConfirmationModal(
				this.deleteConfirmationBody(),
				'Delete Message');
		} catch {
			this.messagePendingDeletion.set(null);
			return;
		}

		if (!confirmed) {
			this.messagePendingDeletion.set(null);
			return;
		}

		try {
			await firstValueFrom(this.apiClient.deleteRecentlyStoredSecretMessage(msg.id));
			void this.messagesResource.reload();
		} catch {
			// The error interceptor already reported the failure.
		} finally {
			this.messagePendingDeletion.set(null);
		}
	}
}
