import { TestBed } from '@angular/core/testing';

import { ClientLocation } from '../models/client-location.model';
import { RecipientLocationService } from './recipient-location.service';

describe('RecipientLocationService', () => {
	let service: RecipientLocationService;
	let originalGeolocation: PropertyDescriptor | undefined;
	let originalPermissions: PropertyDescriptor | undefined;
	let originalUserAgentData: PropertyDescriptor | undefined;

	beforeEach(() => {
		TestBed.configureTestingModule({});
		service = TestBed.inject(RecipientLocationService);

		originalGeolocation = Object.getOwnPropertyDescriptor(navigator, 'geolocation');
		originalPermissions = Object.getOwnPropertyDescriptor(navigator, 'permissions');
		originalUserAgentData = Object.getOwnPropertyDescriptor(navigator, 'userAgentData');
	});

	afterEach(() => {
		restoreNavigatorProperty('geolocation', originalGeolocation);
		restoreNavigatorProperty('permissions', originalPermissions);
		restoreNavigatorProperty('userAgentData', originalUserAgentData);
	});

	function restoreNavigatorProperty(name: string, descriptor: PropertyDescriptor | undefined): void {
		if (descriptor) {
			Object.defineProperty(navigator, name, descriptor);
		} else {
			delete (navigator as unknown as Record<string, unknown>)[name];
		}
	}

	function setGeolocation(getCurrentPosition: Geolocation['getCurrentPosition']): void {
		Object.defineProperty(navigator, 'geolocation', {
			configurable: true,
			value: { getCurrentPosition }
		});
	}

	function setPermissions(state: PermissionState): void {
		Object.defineProperty(navigator, 'permissions', {
			configurable: true,
			value: { query: () => Promise.resolve({ state } as PermissionStatus) }
		});
	}

	it('should request the device location directly so the browser can prompt', async () => {
		const getCurrentPosition = jasmine.createSpy('getCurrentPosition').and.callFake(
			(_success: PositionCallback, error?: PositionErrorCallback) => {
				error?.({ code: 1, message: 'denied' } as GeolocationPositionError);
			});
		setGeolocation(getCurrentPosition);
		setPermissions('prompt');

		const result = await service.collect();

		expect(getCurrentPosition).toHaveBeenCalled();
		expect(result.latitude).toBeUndefined();
		expect(result.timezone).toBeTruthy();
		expect(result.utcOffsetMinutes).toBeDefined();
	});

	it('should not request device location when the browser has blocked it', async () => {
		const getCurrentPosition = jasmine.createSpy('getCurrentPosition');
		setGeolocation(getCurrentPosition);
		setPermissions('denied');

		const result = await service.collect();

		expect(getCurrentPosition).not.toHaveBeenCalled();
		expect(result.latitude).toBeUndefined();
		expect(result.longitude).toBeUndefined();
	});

	it('should round device coordinates to three decimals', async () => {
		setPermissions('prompt');
		setGeolocation((success: PositionCallback) => {
			success(<GeolocationPosition>{
				coords: { latitude: 44.42678, longitude: 26.10249 }
			});
		});

		const result = await service.collect();

		expect(result.latitude).toBe(44.427);
		expect(result.longitude).toBe(26.102);
	});

	it('should fall back to request context when the device lookup fails', async () => {
		setPermissions('prompt');
		setGeolocation((_success: PositionCallback, error?: PositionErrorCallback) => {
			error?.({ code: 1, message: 'denied' } as GeolocationPositionError);
		});

		const result = await service.collect();

		expect(result.latitude).toBeUndefined();
		expect(result.longitude).toBeUndefined();
		expect(result.timezone).toBeTruthy();
	});

	it('should wait for the browser prompt to be answered', async () => {
		setPermissions('prompt');
		let positionCallback: PositionCallback | undefined;
		setGeolocation((success: PositionCallback) => {
			// The prompt stays open, so the browser does not answer yet.
			positionCallback = success;
		});

		let result: ClientLocation | undefined;
		void service.collect().then(value => {
			result = value;
		});

		await new Promise(resolve => setTimeout(resolve));

		// No timeout is allowed to bypass the recipient's decision.
		expect(positionCallback).toBeDefined();
		expect(result).toBeUndefined();

		positionCallback!(<GeolocationPosition>{
			coords: { latitude: 44.42678, longitude: 26.10249 }
		});
		await new Promise(resolve => setTimeout(resolve));

		expect(result?.latitude).toBe(44.427);
		expect(result?.longitude).toBe(26.102);
	});

	it('should report that a permission prompt is expected', async () => {
		setPermissions('prompt');
		setGeolocation(() => {
			// The browser keeps the prompt open; the callback is not needed here.
		});

		const onPermissionPrompt = jasmine.createSpy('onPermissionPrompt');
		void service.collect(onPermissionPrompt);

		await new Promise(resolve => setTimeout(resolve));

		expect(onPermissionPrompt).toHaveBeenCalled();
	});

	it('should not report a prompt when permission was already granted', async () => {
		setPermissions('granted');
		setGeolocation((success: PositionCallback) => {
			success(<GeolocationPosition>{
				coords: { latitude: 44.42678, longitude: 26.10249 }
			});
		});

		const onPermissionPrompt = jasmine.createSpy('onPermissionPrompt');
		await service.collect(onPermissionPrompt);

		expect(onPermissionPrompt).not.toHaveBeenCalled();
	});

	it('should not report a prompt when permission was already denied', async () => {
		setPermissions('denied');
		const getCurrentPosition = jasmine.createSpy('getCurrentPosition');
		setGeolocation(getCurrentPosition);

		const onPermissionPrompt = jasmine.createSpy('onPermissionPrompt');
		await service.collect(onPermissionPrompt);

		expect(onPermissionPrompt).not.toHaveBeenCalled();
		expect(getCurrentPosition).not.toHaveBeenCalled();
	});

	it('should tolerate missing geolocation support', async () => {
		Object.defineProperty(navigator, 'geolocation', { configurable: true, value: {} });

		const result = await service.collect();

		expect(result.latitude).toBeUndefined();
		expect(result.timezone).toBeTruthy();
	});

	it('should include the device model when the browser exposes it', async () => {
		setPermissions('denied');
		Object.defineProperty(navigator, 'userAgentData', {
			configurable: true,
			value: { getHighEntropyValues: () => Promise.resolve({ model: 'Pixel 7' }) }
		});

		const result = await service.collect();

		expect(result.deviceModel).toBe('Pixel 7');
	});

	it('should omit the device model when the browser does not expose one', async () => {
		setPermissions('denied');
		Object.defineProperty(navigator, 'userAgentData', {
			configurable: true,
			value: { getHighEntropyValues: () => Promise.resolve({ model: '' }) }
		});

		const result = await service.collect();

		expect(result.deviceModel).toBeUndefined();
	});

	it('should tolerate a failing device model lookup', async () => {
		setPermissions('denied');
		Object.defineProperty(navigator, 'userAgentData', {
			configurable: true,
			value: { getHighEntropyValues: () => Promise.reject(new Error('unsupported')) }
		});

		const result = await service.collect();

		expect(result.deviceModel).toBeUndefined();
	});
});
