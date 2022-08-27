import { Injectable } from '@angular/core';
import * as FileSaver from 'file-saver';

@Injectable({
	providedIn: 'root'
})
export class FileService {

	constructor() { }

	readFileAsBase64Blob(file: File): Promise<string> {
		return new Promise<string>((resolve, reject) => {
			const fileReader = new FileReader();

			fileReader.onload = () => {
				resolve(fileReader.result as string);
			};

			fileReader.onerror = reject;

			fileReader.readAsDataURL(file);
		})
	}

	saveBase64BlobAsFile(base64Blob: string, saveFileName: string) {
		FileSaver.saveAs(base64Blob, saveFileName);
	}
}
