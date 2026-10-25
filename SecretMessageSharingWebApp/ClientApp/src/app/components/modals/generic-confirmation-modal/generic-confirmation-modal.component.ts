import { NgTemplateOutlet } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, TemplateRef } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';

import { MODAL_DATA } from '../../../tokens';

export interface GenericConfirmationModalData {
	bodyTemplateRef: TemplateRef<unknown>;
	title: string;
}

@Component({
	templateUrl: './generic-confirmation-modal.component.html',
	changeDetection: ChangeDetectionStrategy.OnPush,
	imports: [NgTemplateOutlet]
})
export class GenericConfirmationModalComponent {
	readonly data = inject(MODAL_DATA) as GenericConfirmationModalData;

	readonly modal = inject(NgbActiveModal);

	resultOk(): void {
		this.modal.close(true);
	}

	resultCancel(): void {
		this.modal.close(false);
	}
}
