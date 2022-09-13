import { Component } from '@angular/core';
import { SecretMessageDeliveryNotificationHubService } from '../../services/secret-message-delivery-notification-hub.service';

@Component({
	selector: 'app-root',
	templateUrl: './app-root.component.html',
	styleUrls: ['./app-root.component.css']
})
export class AppRootComponent {

	constructor(secretMessageDeliveryNotificationHubService: SecretMessageDeliveryNotificationHubService) { }

}
