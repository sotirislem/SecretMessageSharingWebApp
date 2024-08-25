import * as sjcl from 'sjcl';
import { Injectable } from '@angular/core';
import { SecretMessageData } from '../models/common/secret-message-data.model';
import { DecryptionResult, SjclDecryptionResult } from '../models/sjcl-decryption-result.model';
import { SecretMessage } from '../models/secret-message.model';
import { SjclEncryptionResult } from '../models/sjcl-encryption-result.model';

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

	encryptMessage(secretMsgObj: SecretMessage): SjclEncryptionResult {
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

		const encryptionKeyAsBase64url = this.convertEncryptionKeyToBase64Url(encryptionKey);
		const encryptionKeySha256 = this.getSha256(encryptionKeyAsBase64url);


		const sjclDecryptionResult = <SjclEncryptionResult>{
			secretMessageData,
			encryptionKeyAsBase64url,
			encryptionKeySha256
		};

		return sjclDecryptionResult;
	}

	decryptMessage(secretMessageData: SecretMessageData, encryptionKeyAsBase64url: string): SjclDecryptionResult {
		const encryptionKey = this.convertBase64UrlToEncryptionKey(encryptionKeyAsBase64url);
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

	getSha256(value: string): string {
		return sjcl.codec.hex.fromBits(sjcl.hash.sha256.hash(value));
	}

	private generateEncryptionParams(): sjcl.SjclCipherEncryptParams {
		return <sjcl.SjclCipherEncryptParams>{
			...this.encryptionSettings,
			iv: sjcl.random.randomWords(4),		//128 bits
			salt: sjcl.random.randomWords(4)	//128 bits
		};
	}

	private generateEncryptionKey(): sjcl.BitArray {
		return sjcl.random.randomWords(8);		//256 bits
	}

	private convertEncryptionKeyToBase64Url(encryptionKey: sjcl.BitArray): string {
		return sjcl.codec.base64url.fromBits(encryptionKey);
	}

	private convertBase64UrlToEncryptionKey(encryptionKeyAsBase64url: string): sjcl.BitArray {
		return sjcl.codec.base64url.toBits(encryptionKeyAsBase64url);
	}
}
