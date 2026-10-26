import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { API_URL } from '../tokens';
import { StoreNewSecretMessageRequest } from '../models/api/store-new-secret-message-request.model';
import { ApiClientService } from './api-client.service';

describe('ApiClientService', () => {
	const apiUrl = 'https://api.example.com';
	const baseApiUrl = `${apiUrl}/api/secret-messages`;

	let service: ApiClientService;
	let httpTesting: HttpTestingController;

	beforeEach(() => {
		TestBed.configureTestingModule({
			providers: [
				provideHttpClient(),
				provideHttpClientTesting(),
				{ provide: API_URL, useValue: apiUrl }
			]
		});

		service = TestBed.inject(ApiClientService);
		httpTesting = TestBed.inject(HttpTestingController);
	});

	afterEach(() => {
		httpTesting.verify();
	});

	it('should store a secret message with a POST request', () => {
		const request = <StoreNewSecretMessageRequest>{
			secretMessageData: { iv: 'iv', ct: 'ct' },
			otp: { required: false, recipientsEmail: '' },
			encryptionKeySha256: 'hash'
		};
		let result: string | undefined;

		service.storeSecretMessage(request).subscribe(value => result = value);

		const httpRequest = httpTesting.expectOne(baseApiUrl);
		expect(httpRequest.request.method).toBe('POST');
		expect(httpRequest.request.body).toEqual(request);

		httpRequest.flush('message-id');

		expect(result).toBe('message-id');
	});

	it('should verify a secret message', () => {
		let result: unknown;

		service.verifySecretMessage('message-id').subscribe(value => result = value);

		const httpRequest = httpTesting.expectOne(`${baseApiUrl}/verify/message-id`);
		expect(httpRequest.request.method).toBe('GET');

		const response = { id: 'message-id', exists: true };
		httpRequest.flush(response);

		expect(result).toEqual(response);
	});

	it('should acquire an OTP for a message', () => {
		service.acquireSecretMessageOtp('message-id').subscribe();

		const httpRequest = httpTesting.expectOne(`${baseApiUrl}/otp/message-id`);
		expect(httpRequest.request.method).toBe('GET');
		expect(httpRequest.request.headers.has('X-Client-Timezone')).toBeFalse();
		expect(httpRequest.request.headers.has('X-Client-Utc-Offset-Minutes')).toBeFalse();

		httpRequest.flush(null);
	});

	it('should send the time zone context when acquiring an OTP', () => {
		service.acquireSecretMessageOtp('message-id', {
			timezone: 'Europe/Bucharest',
			utcOffsetMinutes: 180,
			language: 'ro-RO'
		}).subscribe();

		const httpRequest = httpTesting.expectOne(`${baseApiUrl}/otp/message-id`);
		expect(httpRequest.request.method).toBe('GET');
		expect(httpRequest.request.headers.get('X-Client-Timezone')).toBe('Europe/Bucharest');
		expect(httpRequest.request.headers.get('X-Client-Utc-Offset-Minutes')).toBe('180');
		expect(httpRequest.request.headers.get('X-Client-Language')).toBe('ro-RO');
		expect(httpRequest.request.headers.has('X-Client-Latitude')).toBeFalse();
		expect(httpRequest.request.headers.has('X-Client-Longitude')).toBeFalse();

		httpRequest.flush(null);
	});

	it('should validate an OTP by posting the code', () => {
		let result: unknown;

		service.validateSecretMessageOtp('message-id', '12345678').subscribe(value => result = value);

		const httpRequest = httpTesting.expectOne(`${baseApiUrl}/otp/message-id`);
		expect(httpRequest.request.method).toBe('POST');
		expect(httpRequest.request.body).toEqual({ otpCode: '12345678' });

		const response = { isValid: true, canRetry: false, hasExpired: false, authToken: 'token' };
		httpRequest.flush(response);

		expect(result).toEqual(response);
	});

	it('should send the key hash in a header instead of the URL', () => {
		service.getSecretMessage('message-id', 'key-hash').subscribe();

		const httpRequest = httpTesting.expectOne(`${baseApiUrl}/message-id`);
		expect(httpRequest.request.method).toBe('GET');
		expect(httpRequest.request.headers.get('X-Key-Hash')).toBe('key-hash');
		expect(httpRequest.request.headers.has('Authorization')).toBeFalse();
		expect(httpRequest.request.url).not.toContain('key-hash');

		httpRequest.flush({ data: { iv: 'iv', ct: 'ct' }, deliveryNotificationSent: true });
	});

	it('should add a bearer token when one is supplied', () => {
		service.getSecretMessage('message-id', 'key-hash', 'auth-token').subscribe();

		const httpRequest = httpTesting.expectOne(`${baseApiUrl}/message-id`);
		expect(httpRequest.request.headers.get('Authorization')).toBe('Bearer auth-token');

		httpRequest.flush({ data: { iv: 'iv', ct: 'ct' }, deliveryNotificationSent: false });
	});

	it('should send location and request context as headers', () => {
		service.getSecretMessage('message-id', 'key-hash', undefined, {
			latitude: 44.4268,
			longitude: 26.1025,
			timezone: 'Europe/Bucharest',
			utcOffsetMinutes: 180,
			language: 'ro-RO',
			deviceModel: 'Pixel 7'
		}).subscribe();

		const httpRequest = httpTesting.expectOne(`${baseApiUrl}/message-id`);
		expect(httpRequest.request.headers.get('X-Client-Latitude')).toBe('44.427');
		expect(httpRequest.request.headers.get('X-Client-Longitude')).toBe('26.102');
		expect(httpRequest.request.headers.get('X-Client-Timezone')).toBe('Europe/Bucharest');
		expect(httpRequest.request.headers.get('X-Client-Utc-Offset-Minutes')).toBe('180');
		expect(httpRequest.request.headers.get('X-Client-Language')).toBe('ro-RO');
		expect(httpRequest.request.headers.get('X-Client-Device-Model')).toBe('Pixel 7');
		expect(httpRequest.request.url).not.toContain('44.427');
		expect(httpRequest.request.url).not.toContain('26.103');

		httpRequest.flush({ data: { iv: 'iv', ct: 'ct' }, deliveryNotificationSent: false });
	});

	it('should not send coordinate headers when no device location is available', () => {
		service.getSecretMessage('message-id', 'key-hash', undefined, {
			timezone: 'Europe/Bucharest',
			utcOffsetMinutes: 180,
			language: 'ro-RO'
		}).subscribe();

		const httpRequest = httpTesting.expectOne(`${baseApiUrl}/message-id`);
		expect(httpRequest.request.headers.has('X-Client-Latitude')).toBeFalse();
		expect(httpRequest.request.headers.has('X-Client-Longitude')).toBeFalse();
		expect(httpRequest.request.headers.get('X-Client-Timezone')).toBe('Europe/Bucharest');

		httpRequest.flush({ data: { iv: 'iv', ct: 'ct' }, deliveryNotificationSent: false });
	});

	it('should load the recently stored messages', () => {
		let result: unknown;

		service.getRecentlyStoredSecretMessages().subscribe(value => result = value);

		const httpRequest = httpTesting.expectOne(baseApiUrl);
		expect(httpRequest.request.method).toBe('GET');

		const response = { recentlyStoredSecretMessages: [] };
		httpRequest.flush(response);

		expect(result).toEqual(response);
	});

	it('should expose the recently stored messages URL', () => {
		expect(service.getRecentlyStoredSecretMessagesUrl()).toBe(baseApiUrl);
	});

	it('should delete a recently stored message', () => {
		service.deleteRecentlyStoredSecretMessage('message-id').subscribe();

		const httpRequest = httpTesting.expectOne(`${baseApiUrl}/message-id`);
		expect(httpRequest.request.method).toBe('DELETE');

		httpRequest.flush(null);
	});
});
