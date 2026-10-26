import { httpResource } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, computed, inject, signal, TemplateRef, viewChild } from '@angular/core';
import { firstValueFrom } from 'rxjs';

import { DeliveryDetails, RecentlyStoredSecretMessage, RecentlyStoredSecretMessagesResponse } from '../../models/api/recently-stored-secret-messages-response.model';
import { SecretMessageDeliveryNotification } from '../../models/api/secret-message-delivery-notification.model';
import { ApiClientService } from '../../services/api-client.service';
import { ModalService } from '../../services/modal.service';
import { SecretMessageDeliveryNotificationHubService } from '../../services/secret-message-delivery-notification-hub.service';
import { ToastService } from '../../services/toast.service';
import { MessageDeliveryDetailsModalComponent } from '../modals/message-delivery-details-modal/message-delivery-details-modal.component';
import { DateTimeFullPipe } from '../../pipes/date-time-full.pipe';

function toDeliveryDetails(notification: SecretMessageDeliveryNotification): DeliveryDetails {
	return {
		deliveredAt: notification.messageDeliveredOn,
		recipientIP: notification.recipientIP,
		recipientClientInfo: notification.recipientClientInfo,
		recipientCity: notification.recipientCity,
		recipientRegion: notification.recipientRegion,
		recipientCountry: notification.recipientCountry,
		recipientCountryCode: notification.recipientCountryCode,
		recipientLocationSource: notification.recipientLocationSource,
		recipientIsp: notification.recipientIsp,
		recipientTimezone: notification.recipientTimezone,
		recipientUtcOffsetMinutes: notification.recipientUtcOffsetMinutes,
		recipientLanguage: notification.recipientLanguage,
		otpVerified: notification.otpVerified
	};
}

@Component({
	templateUrl: './recently-stored-messages.component.html',
	changeDetection: ChangeDetectionStrategy.OnPush,
	imports: [DateTimeFullPipe]
})
export class RecentlyStoredMessagesComponent {
	private readonly apiClient = inject(ApiClientService);
	private readonly modalService = inject(ModalService);
	private readonly toastService = inject(ToastService);
	private readonly hubService = inject(SecretMessageDeliveryNotificationHubService);

	private readonly messagesResource = httpResource<RecentlyStoredSecretMessagesResponse>(
		() => this.apiClient.getRecentlyStoredSecretMessagesUrl());

	private readonly deliveryNotificationsById = computed(() =>
		new Map<string, SecretMessageDeliveryNotification>(
			this.hubService.deliveryNotifications().map(notification => [notification.messageId, notification])));

	private readonly deleteConfirmationBody = viewChild.required<TemplateRef<unknown>>('deleteConfirmationModalBody');

	readonly messagePendingDeletion = signal<RecentlyStoredSecretMessage | null>(null);
	readonly loading = this.messagesResource.isLoading;
	readonly loadError = this.messagesResource.error;
	// The list is loaded once, but a delivery notification can arrive while it is open.
	// Overlay those notifications so the affected row switches to "Delivered" without a
	// refresh, using the same data the server would have returned.
	readonly recentlyStoredSecretMessages = computed(() =>
		(this.messagesResource.value()?.recentlyStoredSecretMessages ?? [])
			.map(msg => {
				if (msg.deliveryDetails) return msg;

				const notification = this.deliveryNotificationsById().get(msg.id);
				return notification
					? { ...msg, deliveryDetails: toDeliveryDetails(notification) }
					: msg;
			}));
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
		const deliveryDetails = msg.deliveryDetails!;
		const messageDeliveryDetails = <SecretMessageDeliveryNotification>{
			messageId: msg.id,
			messageCreatedOn: msg.createdDateTime,
			messageDeliveredOn: deliveryDetails.deliveredAt,
			recipientIP: deliveryDetails.recipientIP,
			recipientClientInfo: deliveryDetails.recipientClientInfo,
			recipientCity: deliveryDetails.recipientCity,
			recipientRegion: deliveryDetails.recipientRegion,
			recipientCountry: deliveryDetails.recipientCountry,
			recipientCountryCode: deliveryDetails.recipientCountryCode,
			recipientLocationSource: deliveryDetails.recipientLocationSource,
			recipientIsp: deliveryDetails.recipientIsp,
			recipientTimezone: deliveryDetails.recipientTimezone,
			recipientUtcOffsetMinutes: deliveryDetails.recipientUtcOffsetMinutes,
			recipientLanguage: deliveryDetails.recipientLanguage,
			otpVerified: deliveryDetails.otpVerified
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
