import { HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';

import { Constants } from '../../constants';
import { OtpSettings, StoreNewSecretMessageRequest } from '../models/api/store-new-secret-message-request.model';
import { SecretMessage } from '../models/secret-message.model';
import { ApiClientService } from './api-client.service';
import { CryptoService } from './crypto.service';
import { ToastService } from './toast.service';

export interface CreatedSecretMessage {
	secretMsgId: string;
	secretMsgKey: string;
}

/**
 * Encrypts a message locally and stores the ciphertext.
 *
 * Every call generates a fresh encryption key, so the same content can be stored
 * several times (for example when the sender wants another single-use link)
 * without the links being interchangeable.
 */
@Injectable({
	providedIn: 'root'
})
export class SecretMessageCreationService {
	private readonly cryptoService = inject(CryptoService);
	private readonly apiClientService = inject(ApiClientService);
	private readonly toastService = inject(ToastService);

	/**
	 * Returns the stored message on success and null on failure, after reporting
	 * the failure to the user.
	 */
	async create(
		secretMsgObj: SecretMessage,
		otp: OtpSettings,
		notificationEmail: string | null = null): Promise<CreatedSecretMessage | null> {
		try {
			const encryptionResult = await this.cryptoService.encryptMessage(secretMsgObj);

			// The encrypted payload is what gets stored, so check its size after encryption.
			// This covers the message text, attachment, filename, credentials, and Unicode/JSON escaping.
			if (encryptionResult.secretMessageData.ct.length > Constants.MAX_ENCRYPTED_PAYLOAD_BASE64_LENGTH) {
				this.toastService.warning(
					'The encrypted message is too large to store. Reduce the message text or attach a smaller file.',
					'Message too large'
				);
				return null;
			}

			const request = <StoreNewSecretMessageRequest>{
				secretMessageData: encryptionResult.secretMessageData,
				otp,
				encryptionKeySha256: encryptionResult.encryptionKeySha256,
				notificationEmail
			};

			const secretMsgId = await firstValueFrom(this.apiClientService.storeSecretMessage(request));

			return {
				secretMsgId,
				secretMsgKey: encryptionResult.encryptionKeyAsBase64Url
			};
		} catch (error) {
			// HTTP failures have already been reported by httpErrorInterceptor.
			if (!(error instanceof HttpErrorResponse)) {
				this.toastService.error('Could not encrypt the message. Please try again.');
			}

			return null;
		}
	}
}
