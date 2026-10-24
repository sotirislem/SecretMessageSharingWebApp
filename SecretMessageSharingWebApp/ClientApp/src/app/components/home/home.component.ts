import { ChangeDetectionStrategy, Component } from '@angular/core';

import { CreateSecretMessageComponent } from '../create-secret-message/create-secret-message.component';

@Component({
	selector: 'app-home',
	templateUrl: './home.component.html',
	styleUrl: './home.component.css',
	changeDetection: ChangeDetectionStrategy.OnPush,
	imports: [CreateSecretMessageComponent]
})
export class HomeComponent {
}
