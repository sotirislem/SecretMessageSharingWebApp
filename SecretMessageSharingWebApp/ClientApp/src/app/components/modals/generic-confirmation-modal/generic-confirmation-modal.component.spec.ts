import { Component, TemplateRef, ViewChild } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';

import { MODAL_DATA } from '../../../tokens';
import {
	GenericConfirmationModalComponent,
	GenericConfirmationModalData
} from './generic-confirmation-modal.component';

@Component({
	standalone: true,
	template: `
		<ng-template #body>
			<p class="confirmation-body">Are you sure you want to continue?</p>
		</ng-template>
	`
})
class HostComponent {
	@ViewChild('body', { read: TemplateRef }) body!: TemplateRef<unknown>;
}

describe('GenericConfirmationModalComponent', () => {
	let modal: jasmine.SpyObj<NgbActiveModal>;
	let fixture: ReturnType<typeof TestBed.createComponent<GenericConfirmationModalComponent>>;

	beforeEach(async () => {
		modal = jasmine.createSpyObj<NgbActiveModal>('NgbActiveModal', ['close', 'dismiss']);

		const data: GenericConfirmationModalData = {
			bodyTemplateRef: null as unknown as TemplateRef<unknown>,
			title: 'Delete Message'
		};

		await TestBed.configureTestingModule({
			imports: [HostComponent, GenericConfirmationModalComponent],
			providers: [
				{ provide: MODAL_DATA, useValue: data },
				{ provide: NgbActiveModal, useValue: modal }
			]
		}).compileComponents();

		const hostFixture = TestBed.createComponent(HostComponent);
		hostFixture.detectChanges();
		data.bodyTemplateRef = hostFixture.componentInstance.body;

		fixture = TestBed.createComponent(GenericConfirmationModalComponent);
		fixture.detectChanges();
	});

	it('should render the title and the supplied body template', () => {
		const element: HTMLElement = fixture.nativeElement;
		expect(element.querySelector('.modal-title')?.textContent).toContain('Delete Message');
		expect(element.querySelector('.confirmation-body')?.textContent).toContain('Are you sure');
	});

	it('should close the modal with true when confirmed', () => {
		const confirm = fixture.nativeElement.querySelector('.btn-primary') as HTMLButtonElement;

		confirm.click();

		expect(modal.close).toHaveBeenCalledWith(true);
	});

	it('should close the modal with false when cancelled', () => {
		const cancel = fixture.nativeElement.querySelector('.btn-outline-secondary') as HTMLButtonElement;

		cancel.click();

		expect(modal.close).toHaveBeenCalledWith(false);
	});
});
