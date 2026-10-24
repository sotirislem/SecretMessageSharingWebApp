import { TemplateRef } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { NgbModal, NgbModalOptions, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { Subject } from 'rxjs';

import { GenericConfirmationModalComponent } from '../components/modals/generic-confirmation-modal/generic-confirmation-modal.component';
import { MODAL_DATA } from '../tokens';
import { ModalService } from './modal.service';

describe('ModalService', () => {
	let service: ModalService;
	let ngbModal: jasmine.SpyObj<NgbModal>;
	let hiddenSubject: Subject<void>;
	let resolveResult: (value: unknown) => void;
	let rejectResult: (reason: unknown) => void;

	beforeEach(() => {
		hiddenSubject = new Subject<void>();

		const result = new Promise((resolve, reject) => {
			resolveResult = resolve;
			rejectResult = reject;
		});

		const modalRef = {
			result,
			hidden: hiddenSubject.asObservable()
		} as NgbModalRef;

		ngbModal = jasmine.createSpyObj<NgbModal>('NgbModal', ['open']);
		ngbModal.open.and.returnValue(modalRef);

		TestBed.configureTestingModule({
			providers: [{ provide: NgbModal, useValue: ngbModal }]
		});

		service = TestBed.inject(ModalService);
	});

	it('should open the requested component with the supplied data', () => {
		const content = class TestModal { };
		const data = { value: 1 };

		void service.openModal(content, data);

		expect(ngbModal.open).toHaveBeenCalled();
		const [openedContent, options] = ngbModal.open.calls.mostRecent().args as [unknown, NgbModalOptions];
		expect(openedContent).toBe(content);
		expect(options.injector).toBeDefined();
		expect(options.injector!.get(MODAL_DATA)).toBe(data);
	});

	it('should resolve with the modal result after the closing animation finished', async () => {
		const promise = service.openModal<string, object>(class TestModal { }, {});

		resolveResult('result');

		let resolved = false;
		void promise.then(() => resolved = true);
		await Promise.resolve();
		expect(resolved).toBeFalse();

		hiddenSubject.next();

		await expectAsync(promise).toBeResolvedTo('result');
	});

	it('should resolve when the hidden observable completes without emitting', async () => {
		const promise = service.openModal<string, object>(class TestModal { }, {});

		resolveResult('result');
		hiddenSubject.complete();

		await expectAsync(promise).toBeResolvedTo('result');
	});

	it('should reject when the modal is dismissed', async () => {
		const promise = service.openModal<string, object>(class TestModal { }, {});

		rejectResult('dismissed');

		await expectAsync(promise).toBeRejectedWith('dismissed');
	});

	it('should open the generic confirmation modal with its template and options', () => {
		const bodyTemplateRef = {} as TemplateRef<unknown>;

		void service.openConfirmationModal(bodyTemplateRef, 'Delete Message');

		const [content, options] = ngbModal.open.calls.mostRecent().args as [unknown, NgbModalOptions];
		expect(content).toBe(GenericConfirmationModalComponent);
		expect(options.centered).toBeTrue();
		expect(options.backdrop).toBe('static');
		expect(options.injector!.get(MODAL_DATA)).toEqual({
			bodyTemplateRef,
			title: 'Delete Message'
		});
	});

	it('should use a default confirmation title', () => {
		void service.openConfirmationModal({} as TemplateRef<unknown>);

		const options = ngbModal.open.calls.mostRecent().args[1] as NgbModalOptions;
		expect((options.injector!.get(MODAL_DATA) as { title: string }).title).toBe('Confirmation');
	});
});
