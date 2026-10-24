import { TestBed } from '@angular/core/testing';

import { BASE_URL } from '../tokens';
import { UrlHelperService } from './url-helper.service';

describe('UrlHelperService', () => {
	let service: UrlHelperService;

	beforeEach(() => {
		TestBed.configureTestingModule({
			providers: [{ provide: BASE_URL, useValue: 'https://example.com' }]
		});
		service = TestBed.inject(UrlHelperService);
	});

	it('should build a URL with query parameters and a fragment', () => {
		const result = service.createLocalUrlWithParams(
			'get-secret-message',
			{ id: 'message-id' },
			'message-key');

		expect(result).toBe('https://example.com/get-secret-message?id=message-id#message-key');
	});

	it('should omit the fragment when none is provided', () => {
		const result = service.createLocalUrlWithParams(
			'get-secret-message',
			{ id: 'message-id' });

		expect(result).toBe('https://example.com/get-secret-message?id=message-id');
		expect(result).not.toContain('#');
	});

	it('should URL-encode query parameter values', () => {
		const result = service.createLocalUrlWithParams(
			'get-secret-message',
			{ id: 'a b&c' });

		const url = new URL(result);
		expect(url.searchParams.get('id')).toBe('a b&c');
	});

	it('should add every supplied query parameter', () => {
		const result = service.createLocalUrlWithParams(
			'some-route',
			{ first: '1', second: '2' });

		const url = new URL(result);
		expect(url.searchParams.get('first')).toBe('1');
		expect(url.searchParams.get('second')).toBe('2');
	});
});
