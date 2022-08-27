import * as sjcl from 'sjcl';
import { Injectable } from '@angular/core';
import { SecretMessageData } from '../models/common/secret-message-data.model';
import { DecryptionResult, SjclDecryptionResult } from '../models/sjcl-decryption-result.model';
import { SecretMessage } from '../models/secret-message.model';

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

	encryptMessage(secretMsgObj: SecretMessage): [secretMessageData: SecretMessageData, encryptionKey: string] {
		const secretMsgSerialized = JSON.stringify(secretMsgObj);

		const encryptionParams = this.generateEncryptionParams();
		const encryptionKey = this.generateEncryptionKey();

		const encryptedMsgData = sjcl.encrypt(encryptionKey, secretMsgSerialized, encryptionParams) as unknown as string;
		const encryptedMsgJson = JSON.parse(encryptedMsgData) as sjcl.SjclCipherEncrypted;
		const { iv, salt, ct } = encryptedMsgJson;

		const secretMessageData = <SecretMessageData>{
			iv: iv.toString(),
			salt: salt.toString(),
			ct: ct.toString()
		};
		return [secretMessageData, encryptionKey];
	}

	decryptMessage(secretMessageData: SecretMessageData, encryptionKey: string): SjclDecryptionResult {
		const cipherEncrypted = <sjcl.SjclCipherEncrypted>{
			...this.encryptionSettings,
			iv: secretMessageData.iv as unknown,
			salt: secretMessageData.salt as unknown,
			ct: secretMessageData.ct as unknown
		}

		let sjclDecryptionResult = {} as SjclDecryptionResult;
		try {
			const decryptedMsgSerialized = sjcl.decrypt(encryptionKey, JSON.stringify(cipherEncrypted));
			const decryptedMsg = JSON.parse(decryptedMsgSerialized) as SecretMessage;

			sjclDecryptionResult.decryptedMsg = decryptedMsg;
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
