import { TestBed } from '@angular/core/testing';

import { FileService } from './file.service';

describe('FileService', () => {
	let service: FileService;

	beforeEach(() => {
		TestBed.configureTestingModule({});
		service = TestBed.inject(FileService);
	});

	it('should read a file as a base64 data URL', async () => {
		const file = new File(['hello'], 'greeting.txt', { type: 'text/plain' });

		const result = await service.readFileAsBase64Blob(file);

		expect(result).toBe('data:text/plain;base64,aGVsbG8=');
	});

	it('should read binary files without changing their bytes', async () => {
		const bytes = new Uint8Array([0, 1, 2, 253, 254, 255]);
		const file = new File([bytes], 'binary.bin', { type: 'application/octet-stream' });

		const result = await service.readFileAsBase64Blob(file);

		expect(result).toBe('data:application/octet-stream;base64,AAEC/f7/');
	});

	it('should trigger a download for a base64 blob', () => {
		const link = document.createElement('a');
		const clickSpy = spyOn(link, 'click');
		spyOn(document, 'createElement').and.returnValue(link);

		service.saveBase64BlobAsFile('data:text/plain;base64,aGVsbG8=', 'greeting.txt');

		expect(document.createElement).toHaveBeenCalledWith('a');
		expect(link.href).toContain('data:text/plain;base64,aGVsbG8=');
		expect(link.download).toBe('greeting.txt');
		expect(link.rel).toBe('noopener');
		expect(clickSpy).toHaveBeenCalled();
	});
});
