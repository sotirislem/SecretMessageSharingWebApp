import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { NavMenuComponent } from '../nav-menu/nav-menu.component';
import { LoaderComponent } from '../loader/loader.component';
import { ToastContainerComponent } from '../toast-container/toast-container.component';
import { SecretMessageDeliveryNotificationHubService } from '../../services/secret-message-delivery-notification-hub.service';

@Component({
	selector: 'app-root',
	templateUrl: './app-root.component.html',
	styleUrl: './app-root.component.css',
	changeDetection: ChangeDetectionStrategy.OnPush,
	imports: [RouterOutlet, NavMenuComponent, LoaderComponent, ToastContainerComponent]
})
export class AppRootComponent {
	constructor() {
		// Eagerly connect the delivery-notification hub for the lifetime of the app.
		inject(SecretMessageDeliveryNotificationHubService);
	}
}
