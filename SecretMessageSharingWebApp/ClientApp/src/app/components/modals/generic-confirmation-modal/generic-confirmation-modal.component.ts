import { Component, Input, TemplateRef } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { NgTemplateOutlet } from '@angular/common';

@Component({
	templateUrl: './generic-confirmation-modal.component.html',
	styleUrls: ['./generic-confirmation-modal.component.css'],
	standalone: true,
	imports: [NgTemplateOutlet]
})
export class GenericConfirmationModalComponent {

	@Input() bodyTemplateRef: TemplateRef<any>;
	@Input() title: string;

	constructor(public modal: NgbActiveModal) { }

	resultOk() {
		this.modal.close(true);
	}

	resultCancel() {
		this.modal.close(false);
	}
}
