import { Injectable } from '@angular/core';
import * as sjcl from 'sjcl';
import { Constants } from '../../constants';
import { SecretMessage } from '../models/SecretMessage';

@Injectable({
	providedIn: 'root'
})
export class SjclService {

	readonly encryptionSettings = <sjcl.SjclCipherParams>{
		cipher: "aes",	// encryption algorithm
		mode: "gcm",	// block mode
		ks: 256,		// key bits (max 256)
		ts: 128,		// tag bits (max 128)
		iter: 100_000	// PBKDF2 iterations
	};

	constructor() {
		sjcl.random.startCollectors();
	}

	async encryptMessage(msg: string): Promise<SecretMessage> {
		const encryptionParams = this.generateEncryptionParams();
		const encryptionKey = this.generateEncryptionKey();

		const encryptedMsgData = await sjcl.encrypt(encryptionKey, msg, encryptionParams) as unknown as string;
		const encryptedMsgJson = JSON.parse(encryptedMsgData) as sjcl.SjclCipherEncrypted;
		const { iv, salt, ct } = encryptedMsgJson;

		const secretMessage = <SecretMessage>{
			iv: iv.toString(),
			salt: salt.toString(),
			ct: ct.toString()
		};
		return secretMessage;
	}

	async decryptMessage(encryptionKey: string, secretMessage: SecretMessage): Promise<string> {
		const cipherEncrypted = <sjcl.SjclCipherEncrypted>{
			...this.encryptionSettings,
			iv: secretMessage.iv as unknown,
			salt: secretMessage.salt as unknown,
			ct: secretMessage.ct as unknown
		}

		const decryptedMsg: string = await sjcl.decrypt(encryptionKey, JSON.stringify(cipherEncrypted));
		return decryptedMsg;
	}

	retrieveEncryptionKeyFromSessionStorage(): string | null {
		const encryptionKey = sessionStorage.getItem(Constants.ENCRYPTION_KEY_NAME);
		if (encryptionKey) sessionStorage.removeItem(Constants.ENCRYPTION_KEY_NAME);

		return encryptionKey;
	}

	private generateEncryptionParams(): sjcl.SjclCipherEncryptParams {
		return <sjcl.SjclCipherEncryptParams>{
			...this.encryptionSettings,
			iv: sjcl.random.randomWords(4),
			salt: sjcl.random.randomWords(4)
		};
	}

	private generateEncryptionKey(): string {
		const randomBits = sjcl.random.randomWords(10);
		const b64Key = sjcl.codec.base64.fromBits(randomBits);

		const urlSafeKey = b64Key.replace(/[+=\/]/g, '');
		this.addEncryptionKeyToSessionStorage(urlSafeKey);

		return urlSafeKey;
	}

	private addEncryptionKeyToSessionStorage(encryptionKey: string) {
		sessionStorage.setItem(Constants.ENCRYPTION_KEY_NAME, encryptionKey);
	}

}
