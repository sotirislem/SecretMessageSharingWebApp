import { DecryptionStatus } from '../models/decryption-result.model';
import { SecretMessage } from '../models/secret-message.model';

import { CryptoService } from './crypto.service';

describe('CryptoService', () => {
	let service: CryptoService;

	const message: SecretMessage = {
		plainText: 'A secret message with unicode: ✓ 🔐',
		containsFile: false,
		fileName: null,
		base64BlobFile: null,
		containsUsernameAndPassword: true,
		username: 'user',
		password: 'pass'
	};

	beforeEach(() => {
		service = new CryptoService();
	});

	it('should encrypt and decrypt a message with AES-256-GCM', async () => {
		const encryptionResult = await service.encryptMessage(message);

		expect(encryptionResult.secretMessageData.iv).toBeTruthy();
		expect(encryptionResult.secretMessageData.ct).toBeTruthy();
		expect(encryptionResult.encryptionKeyAsBase64Url).toMatch(/^[A-Za-z0-9_-]{43}$/);
		expect(encryptionResult.encryptionKeySha256).toMatch(/^[a-f0-9]{64}$/);

		const decryptionResult = await service.decryptMessage(
			encryptionResult.secretMessageData,
			encryptionResult.encryptionKeyAsBase64Url);

		expect(decryptionResult.status).toBe(DecryptionStatus.OK);
		expect(decryptionResult.decryptedMsg).toEqual(message);
	});

	it('should generate a unique key and IV for every message', async () => {
		const firstResult = await service.encryptMessage(message);
		const secondResult = await service.encryptMessage(message);

		expect(firstResult.secretMessageData.iv).not.toBe(secondResult.secretMessageData.iv);
		expect(firstResult.encryptionKeyAsBase64Url).not.toBe(secondResult.encryptionKeyAsBase64Url);
	});

	it('should reject tampered ciphertext', async () => {
		const encryptionResult = await service.encryptMessage(message);

		const tamperedCipherText = {
			...encryptionResult.secretMessageData,
			ct: tamperBase64(encryptionResult.secretMessageData.ct)
		};

		const decryptionResult = await service.decryptMessage(
			tamperedCipherText,
			encryptionResult.encryptionKeyAsBase64Url);

		expect(decryptionResult.status).toBe(DecryptionStatus.Error);
	});

	it('should reject a different encryption key', async () => {
		const firstResult = await service.encryptMessage(message);
		const secondResult = await service.encryptMessage(message);

		const decryptionResult = await service.decryptMessage(
			firstResult.secretMessageData,
			secondResult.encryptionKeyAsBase64Url);

		expect(decryptionResult.status).toBe(DecryptionStatus.Error);
	});

	it('should reject malformed message data', async () => {
		const decryptionResult = await service.decryptMessage(
			{ iv: 'not-base64', ct: 'not-base64' },
			'not-a-valid-key');

		expect(decryptionResult.status).toBe(DecryptionStatus.Error);
	});

	it('should calculate a lowercase SHA-256 hex digest', async () => {
		const hash = await service.getSha256('abc');

		expect(hash).toBe('ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad');
	});

	it('should reject a non-96-bit initialization vector', async () => {
		const encryptionResult = await service.encryptMessage(message);

		const decryptionResult = await service.decryptMessage(
			{
				...encryptionResult.secretMessageData,
				iv: btoa(String.fromCharCode(...new Uint8Array(16)))
			},
			encryptionResult.encryptionKeyAsBase64Url);

		expect(decryptionResult.status).toBe(DecryptionStatus.Error);
	});

	function tamperBase64(value: string): string {
		const bytes = atob(value).split('').map(character => character.charCodeAt(0));
		bytes[0] ^= 0xFF;

		return btoa(String.fromCharCode(...bytes));
	}
});
