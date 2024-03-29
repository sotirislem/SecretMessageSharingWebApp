import { formatDate } from '@angular/common';
import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
	name: 'dateTimeFull'
})
export class DateTimeFullPipe implements PipeTransform {

	transform(value: string | number | Date): string | null {
		value = new Date(value);
		if (!this.isValidDate(value)) return null;

		return formatDate(value, 'dd/MM/yyyy HH:mm:ss', 'en-US');
	}

	private isValidDate(value: Date) {
		return Object.prototype.toString.call(value) === "[object Date]" && !isNaN(value.getTime());
	}

}
