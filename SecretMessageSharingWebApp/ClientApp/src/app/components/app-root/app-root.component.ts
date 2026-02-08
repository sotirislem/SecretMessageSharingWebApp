import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { NavMenuComponent } from '../nav-menu/nav-menu.component';
import { LoaderComponent } from '../loader/loader.component';
import { SecretMessageDeliveryNotificationHubService } from '../../services/secret-message-delivery-notification-hub.service';

@Component({
	selector: 'app-root',
	templateUrl: './app-root.component.html',
	styleUrls: ['./app-root.component.css'],
	standalone: true,
	imports: [RouterModule, NavMenuComponent, LoaderComponent]
})
export class AppRootComponent {

	constructor(secretMessageDeliveryNotificationHubService: SecretMessageDeliveryNotificationHubService) { }

}
