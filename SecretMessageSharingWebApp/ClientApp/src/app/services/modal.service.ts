import { Injectable, TemplateRef } from '@angular/core';
import { NgbModal, NgbModalOptions } from '@ng-bootstrap/ng-bootstrap';
import { GenericConfirmationModalComponent } from '../components/modals/generic-confirmation-modal/generic-confirmation-modal.component';

@Injectable({
	providedIn: 'root'
})
export class ModalService {

	constructor(private ngbModalService: NgbModal) { }

	openModal<T>(content: any, inputs: { [key: string]: any }, options?: NgbModalOptions): Promise<T> {
		const modal = this.ngbModalService.open(content, options);

		for (let inputObjName in inputs) {
			modal.componentInstance[inputObjName] = inputs[inputObjName];
		}

		return modal.result;
	}

	openConfirmationModal(bodyTemplateRef: TemplateRef<any>, title?: string): Promise<boolean> {
		title ??= "Confirmation";

		return this.openModal<boolean>(
			GenericConfirmationModalComponent,
			{ bodyTemplateRef, title },
			<NgbModalOptions>{ centered: true, backdrop: 'static' }
		);
	}
}
