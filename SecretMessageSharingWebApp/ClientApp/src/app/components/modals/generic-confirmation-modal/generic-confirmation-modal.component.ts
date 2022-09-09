import { Component, Input, TemplateRef } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';

@Component({
	templateUrl: './generic-confirmation-modal.component.html',
	styleUrls: ['./generic-confirmation-modal.component.css']
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
