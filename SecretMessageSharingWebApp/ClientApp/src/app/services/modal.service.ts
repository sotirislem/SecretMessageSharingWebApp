import { inject, Injectable, Injector, TemplateRef, Type } from '@angular/core';
import { NgbModal, NgbModalOptions } from '@ng-bootstrap/ng-bootstrap';

import { GenericConfirmationModalComponent } from '../components/modals/generic-confirmation-modal/generic-confirmation-modal.component';
import { MODAL_DATA } from '../tokens';

@Injectable({
	providedIn: 'root'
})
export class ModalService {
	private readonly ngbModal = inject(NgbModal);
	private readonly injector = inject(Injector);

	/**
	 * Opens a component modal and hands `data` to it through the `MODAL_DATA` token.
	 *
	 * ng-bootstrap creates the component itself, so a signal `input()` cannot be
	 * bound from here. Providing the data in a child injector keeps the contract
	 * typed and avoids mutating the component instance after creation.
	 */
	openModal<TResult, TData>(content: Type<unknown>, data: TData, options?: NgbModalOptions): Promise<TResult> {
		const injector = Injector.create({
			providers: [{ provide: MODAL_DATA, useValue: data }],
			parent: this.injector
		});

		const modal = this.ngbModal.open(content, { ...options, injector });

		// Wait until the closing animation has finished before handing the result to
		// the caller. Otherwise follow-up UI (toasts, another modal) can appear while
		// this modal and its backdrop are still fading out, which looks like flicker.
		return new Promise<TResult>((resolve, reject) => {
			modal.result.then(
				result => modal.hidden.subscribe({
					next: () => resolve(result),
					complete: () => resolve(result)
				}),
				error => reject(error)
			);
		});
	}

	openConfirmationModal(bodyTemplateRef: TemplateRef<unknown>, title = 'Confirmation'): Promise<boolean> {
		return this.openModal<boolean, { bodyTemplateRef: TemplateRef<unknown>; title: string }>(
			GenericConfirmationModalComponent,
			{ bodyTemplateRef, title },
			<NgbModalOptions>{ centered: true, backdrop: 'static' }
		);
	}
}
