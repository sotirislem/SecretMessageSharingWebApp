import { TestBed } from '@angular/core/testing';

import { NewSecretMessageState, NewSecretMessageStateService } from './new-secret-message-state.service';

describe('NewSecretMessageStateService', () => {
	let service: NewSecretMessageStateService;

	const state: NewSecretMessageState = {
		secretMsgObj: {
			plainText: 'secret',
			containsFile: false,
			fileName: null,
			base64BlobFile: null,
			containsUsernameAndPassword: false,
			username: '',
			password: ''
		},
		secretMsgId: 'message-id',
		secretMsgKey: 'message-key',
		otp: { required: false, recipientsEmail: '' }
	};

	beforeEach(() => {
		TestBed.configureTestingModule({});
		service = TestBed.inject(NewSecretMessageStateService);
	});

	it('should return null before any message is stored', () => {
		expect(service.get()).toBeNull();
	});

	it('should keep the stored message in memory', () => {
		service.set(state);

		expect(service.get()).toBe(state);
	});

	it('should replace a previously stored message', () => {
		service.set(state);
		const replacement: NewSecretMessageState = {
			...state,
			secretMsgId: 'another-id',
			secretMsgKey: 'another-key'
		};

		service.set(replacement);

		expect(service.get()).toBe(replacement);
	});

	it('should clear the stored message', () => {
		service.set(state);

		service.clear();

		expect(service.get()).toBeNull();
	});
});
