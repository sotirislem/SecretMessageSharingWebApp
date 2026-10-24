import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { SecretMessageDeliveryNotificationHubService } from '../../services/secret-message-delivery-notification-hub.service';
import { AppRootComponent } from './app-root.component';

describe('AppRootComponent', () => {
	beforeEach(async () => {
		await TestBed.configureTestingModule({
			imports: [AppRootComponent],
			providers: [
				provideRouter([]),
				{ provide: SecretMessageDeliveryNotificationHubService, useValue: {} }
			]
		}).compileComponents();
	});

	it('should render the navigation, outlet and footer', () => {
		const fixture = TestBed.createComponent(AppRootComponent);
		fixture.detectChanges();

		const element: HTMLElement = fixture.nativeElement;
		expect(element.querySelector('app-nav-menu')).not.toBeNull();
		expect(element.querySelector('router-outlet')).not.toBeNull();
		expect(element.querySelector('app-loader')).not.toBeNull();
		expect(element.querySelector('app-toast-container')).not.toBeNull();
		expect(element.querySelector('.app-footer')?.textContent).toContain('Sotiris Lemousias');
	});

	it('should connect the delivery notification hub on creation', () => {
		const hubService = TestBed.inject(SecretMessageDeliveryNotificationHubService);
		const fixture = TestBed.createComponent(AppRootComponent);
		fixture.detectChanges();

		expect(hubService).toBeDefined();
	});
});
