import { Injectable } from '@angular/core';

import { SecretMessageData } from '../models/common/secret-message-data.model';
import { DecryptionResult, DecryptionStatus } from '../models/decryption-result.model';
import { EncryptionResult } from '../models/encryption-result.model';
import { SecretMessage } from '../models/secret-message.model';

const AES_KEY_LENGTH_BYTES = 32;				// AES-256
const AES_GCM_IV_LENGTH_BYTES = 12;				// 96 bits, recommended IV size for GCM
const AES_GCM_TAG_LENGTH_BITS = 128;

const EMPTY_SECRET_MESSAGE: SecretMessage = {
	plainText: '',
	containsFile: false,
	fileName: '',
	base64BlobFile: '',
	containsUsernameAndPassword: false,
	username: '',
	password: ''
};

@Injectable({
	providedIn: 'root'
})
export class CryptoService {

	async encryptMessage(secretMsgObj: SecretMessage): Promise<EncryptionResult> {
		const webCrypto = this.getWebCrypto();
		const secretMsgSerialized = JSON.stringify(secretMsgObj);

		// A fresh, cryptographically secure 256-bit key is generated for every message.
		const encryptionKey = webCrypto.getRandomValues(new Uint8Array(AES_KEY_LENGTH_BYTES));

		// A random IV must never be reused with the same key. Since every message has its own key,
		// a random 96-bit IV provides a negligible collision probability and is NIST-recommended for GCM.
		const iv = webCrypto.getRandomValues(new Uint8Array(AES_GCM_IV_LENGTH_BYTES));

		const aesKey = await webCrypto.subtle.importKey(
			'raw',
			encryptionKey,
			{ name: 'AES-GCM' },
			false,
			['encrypt']
		);

		const encryptedData = new Uint8Array(await webCrypto.subtle.encrypt(
			{ name: 'AES-GCM', iv, tagLength: AES_GCM_TAG_LENGTH_BITS },
			aesKey,
			new TextEncoder().encode(secretMsgSerialized)
		));

		const encryptionKeyAsBase64Url = this.toBase64Url(encryptionKey);

		return {
			secretMessageData: {
				iv: this.toBase64(iv),
				ct: this.toBase64(encryptedData)
			},
			encryptionKeyAsBase64Url,
			encryptionKeySha256: await this.getSha256(encryptionKeyAsBase64Url)
		};
	}

	async decryptMessage(secretMessageData: SecretMessageData, encryptionKeyAsBase64Url: string): Promise<DecryptionResult> {
		try {
			const webCrypto = this.getWebCrypto();

			const encryptionKey = this.fromBase64Url(encryptionKeyAsBase64Url);
			if (encryptionKey.byteLength !== AES_KEY_LENGTH_BYTES) {
				throw new Error('Invalid encryption key length.');
			}

			const iv = this.fromBase64(secretMessageData.iv);
			if (iv.byteLength !== AES_GCM_IV_LENGTH_BYTES) {
				throw new Error('Invalid initialization vector length.');
			}

			const cipherText = this.fromBase64(secretMessageData.ct);
			if (cipherText.byteLength < AES_GCM_TAG_LENGTH_BITS / 8) {
				throw new Error('Invalid cipher text length.');
			}

			const aesKey = await webCrypto.subtle.importKey(
				'raw',
				encryptionKey,
				{ name: 'AES-GCM' },
				false,
				['decrypt']
			);

			const decryptedData = await webCrypto.subtle.decrypt(
				{ name: 'AES-GCM', iv, tagLength: AES_GCM_TAG_LENGTH_BITS },
				aesKey,
				cipherText
			);

			const decryptedMsg = JSON.parse(new TextDecoder().decode(decryptedData)) as SecretMessage;
			if (!this.isSecretMessage(decryptedMsg)) {
				throw new Error('Invalid message payload.');
			}

			return {
				status: DecryptionStatus.OK,
				decryptedMsg,
				errorMsg: ''
			};
		}
		catch {
			return {
				status: DecryptionStatus.Error,
				decryptedMsg: EMPTY_SECRET_MESSAGE,
				errorMsg: 'The message could not be decrypted. It may be corrupted, tampered with, or the encryption key is invalid.'
			};
		}
	}

	async getSha256(value: string): Promise<string> {
		const webCrypto = this.getWebCrypto();
		const hash = new Uint8Array(await webCrypto.subtle.digest('SHA-256', new TextEncoder().encode(value)));

		return Array.from(hash, byte => byte.toString(16).padStart(2, '0')).join('');
	}

	private getWebCrypto(): Crypto {
		const webCrypto = globalThis.crypto;

		// The Web Crypto API is only exposed in secure contexts (HTTPS or localhost).
		if (!webCrypto?.subtle || typeof webCrypto.getRandomValues !== 'function') {
			throw new Error('The Web Crypto API is not available. A modern browser and a secure context (HTTPS) are required.');
		}

		return webCrypto;
	}

	private isSecretMessage(value: unknown): value is SecretMessage {
		if (typeof value !== 'object' || value === null) return false;

		const message = value as Record<string, unknown>;

		return typeof message.plainText === 'string'
			&& typeof message.containsFile === 'boolean'
			&& (message.fileName === null || typeof message.fileName === 'string')
			&& (message.base64BlobFile === null || typeof message.base64BlobFile === 'string')
			&& typeof message.containsUsernameAndPassword === 'boolean'
			&& typeof message.username === 'string'
			&& typeof message.password === 'string';
	}

	private toBase64(bytes: Uint8Array): string {
		let binary = '';
		const chunkSize = 0x8000;

		for (let offset = 0; offset < bytes.length; offset += chunkSize) {
			binary += String.fromCharCode(...bytes.subarray(offset, offset + chunkSize));
		}

		return btoa(binary);
	}

	private fromBase64(base64: string) {
		const binary = atob(base64);
		const bytes = new Uint8Array(binary.length);

		for (let i = 0; i < binary.length; i++) {
			bytes[i] = binary.charCodeAt(i);
		}

		return bytes;
	}

	private toBase64Url(bytes: Uint8Array): string {
		return this.toBase64(bytes)
			.replace(/\+/g, '-')
			.replace(/\//g, '_')
			.replace(/=+$/, '');
	}

	private fromBase64Url(base64Url: string) {
		const base64 = base64Url
			.replace(/-/g, '+')
			.replace(/_/g, '/');

		const padding = (4 - (base64.length % 4)) % 4;

		return this.fromBase64(base64 + '='.repeat(padding));
	}
}
