import { Injectable } from '@angular/core';

@Injectable({
	providedIn: 'root'
})
export class FileService {
	readFileAsBase64Blob(file: File): Promise<string> {
		return new Promise<string>((resolve, reject) => {
			const fileReader = new FileReader();

			fileReader.onload = () => {
				resolve(fileReader.result as string);
			};

			fileReader.onerror = reject;

			fileReader.readAsDataURL(file);
		});
	}

	saveBase64BlobAsFile(base64Blob: string, saveFileName: string): void {
		const link = document.createElement('a');
		link.href = base64Blob;
		link.download = saveFileName;
		link.rel = 'noopener';
		link.click();
	}
}
