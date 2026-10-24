import { DateTimeFullPipe } from './date-time-full.pipe';

describe('DateTimeFullPipe', () => {
	const pipe = new DateTimeFullPipe();
	const date = new Date(2026, 9, 25, 1, 25, 13);

	it('should format a Date using the full date-time pattern', () => {
		expect(pipe.transform(date)).toBe('25/10/2026 01:25:13');
	});

	it('should format an ISO date string without a time zone as local time', () => {
		expect(pipe.transform('2026-10-25T01:25:13')).toBe('25/10/2026 01:25:13');
	});

	it('should format a timestamp in milliseconds', () => {
		expect(pipe.transform(date.getTime())).toBe('25/10/2026 01:25:13');
	});

	it('should return null for an invalid date string', () => {
		expect(pipe.transform('not-a-date')).toBeNull();
	});

	it('should return null for an invalid Date instance', () => {
		expect(pipe.transform(new Date('invalid'))).toBeNull();
	});

	it('should return null for an empty string', () => {
		expect(pipe.transform('')).toBeNull();
	});
});
