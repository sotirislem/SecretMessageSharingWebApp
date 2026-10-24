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

		// Wait until the closing animation has finished before handing the result to
		// the caller. Otherwise follow-up UI (toasts, another modal) can appear while
		// this modal and its backdrop are still fading out, which looks like flicker.
		return new Promise<T>((resolve, reject) => {
			modal.result.then(
				result => modal.hidden.subscribe({
					next: () => resolve(result),
					complete: () => resolve(result)
				}),
				error => reject(error)
			);
		});
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
