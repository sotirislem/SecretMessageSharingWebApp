import './string.extensions';

describe('String.prototype.linkify', () => {

	it('should escape HTML so message content cannot inject markup', () => {
		const result = '<img src=x onerror="alert(1)">'.linkify();

		expect(result).toBe('&lt;img src=x onerror=&quot;alert(1)&quot;&gt;');
	});

	it('should linkify URLs and open them safely', () => {
		const result = 'Visit https://example.com now'.linkify();

		expect(result).toContain('<a href="https://example.com" target="_blank" rel="noopener noreferrer" referrerpolicy="no-referrer">https://example.com</a>');
	});

	it('should linkify email addresses', () => {
		const result = 'Contact test@example.com'.linkify();

		expect(result).toContain('<a href="mailto:test@example.com" referrerpolicy="no-referrer">test@example.com</a>');
	});

	it('should linkify www addresses without a scheme', () => {
		const result = 'Visit www.example.com now'.linkify();

		expect(result).toContain('<a href="https://www.example.com" target="_blank" rel="noopener noreferrer" referrerpolicy="no-referrer">www.example.com</a>');
	});

	it('should keep escaped HTML around linkified content', () => {
		const result = '<b>https://example.com</b>'.linkify();

		expect(result).toBe('&lt;b&gt;<a href="https://example.com" target="_blank" rel="noopener noreferrer" referrerpolicy="no-referrer">https://example.com</a>&lt;/b&gt;');
	});
});
