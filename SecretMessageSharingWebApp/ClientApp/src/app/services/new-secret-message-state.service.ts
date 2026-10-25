import { Injectable, signal } from '@angular/core';

import { OtpSettings } from '../models/api/store-new-secret-message-request.model';
import { SecretMessage } from '../models/secret-message.model';

export interface NewSecretMessageState {
	secretMsgObj: SecretMessage;
	secretMsgId: string;
	secretMsgKey: string;
	otp: OtpSettings;
}

/**
 * Holds the just-created message only in memory.
 *
 * Passing plaintext and the encryption key through Angular's router state would
 * persist them in history.state, where they remain after navigation and are
 * visible to anything that can read the browser history.
 */
@Injectable({
	providedIn: 'root'
})
export class NewSecretMessageStateService {
	private readonly _state = signal<NewSecretMessageState | null>(null);

	set(state: NewSecretMessageState): void {
		this._state.set(state);
	}

	get(): NewSecretMessageState | null {
		return this._state();
	}

	clear(): void {
		this._state.set(null);
	}
}
