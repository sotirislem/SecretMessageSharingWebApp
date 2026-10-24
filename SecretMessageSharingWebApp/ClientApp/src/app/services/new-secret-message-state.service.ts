import { Injectable } from '@angular/core';

import { SecretMessage } from '../models/secret-message.model';

export interface NewSecretMessageState {
	secretMsgObj: SecretMessage;
	secretMsgId: string;
	secretMsgKey: string;
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
	private state: NewSecretMessageState | null = null;

	set(state: NewSecretMessageState): void {
		this.state = state;
	}

	get(): NewSecretMessageState | null {
		return this.state;
	}

	clear(): void {
		this.state = null;
	}
}
