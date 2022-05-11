import * as sjcl from 'sjcl';
import { Injectable } from '@angular/core';
import { SecretMessage } from '../models/secret-message.model';
import { DecryptionResult, SjclDecryptionResult } from '../models/sjcl-decryption-result.model';

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

	encryptMessage(msg: string): [secretMessage: SecretMessage, encryptionKey: string] {
		const encryptionParams = this.generateEncryptionParams();
		const encryptionKey = this.generateEncryptionKey();

		const encryptedMsgData = sjcl.encrypt(encryptionKey, msg, encryptionParams) as unknown as string;
		const encryptedMsgJson = JSON.parse(encryptedMsgData) as sjcl.SjclCipherEncrypted;
		const { iv, salt, ct } = encryptedMsgJson;

		const secretMessage = <SecretMessage>{
			iv: iv.toString(),
			salt: salt.toString(),
			ct: ct.toString()
		};
		return [secretMessage, encryptionKey];
	}

	decryptMessage(secretMessage: SecretMessage, encryptionKey: string): SjclDecryptionResult {
		const cipherEncrypted = <sjcl.SjclCipherEncrypted>{
			...this.encryptionSettings,
			iv: secretMessage.iv as unknown,
			salt: secretMessage.salt as unknown,
			ct: secretMessage.ct as unknown
		}

		let sjclDecryptionResult = {} as SjclDecryptionResult;
		try {
			sjclDecryptionResult.decryptedMsg = sjcl.decrypt(encryptionKey, JSON.stringify(cipherEncrypted));
			sjclDecryptionResult.result = DecryptionResult.OK;
		}
		catch (e: any) {
			sjclDecryptionResult.errorMsg = e.message;
			sjclDecryptionResult.result = DecryptionResult.Error;
		}
		finally {
			return sjclDecryptionResult;
		}
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

		const urlSafeKey = b64Key.replace(/[+=\/]/g, '').substr(0, 50);

		return urlSafeKey;
	}
}
