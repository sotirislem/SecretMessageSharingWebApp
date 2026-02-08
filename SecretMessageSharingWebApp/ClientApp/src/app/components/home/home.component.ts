import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CreateSecretMessageComponent } from '../create-secret-message/create-secret-message.component';
import { Routes } from '../../../constants';
@Component({
	selector: 'app-home',
	templateUrl: './home.component.html',
	styleUrls: ['./home.component.css'],
	standalone: true,
	imports: [RouterModule, CreateSecretMessageComponent]
})
export class HomeComponent {
	readonly Routes = Routes;

	constructor() { }
}
