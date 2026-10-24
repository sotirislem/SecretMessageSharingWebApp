import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';

import { Routes } from '../../../constants';

@Component({
	selector: 'app-home',
	templateUrl: './home.component.html',
	styleUrl: './home.component.css',
	changeDetection: ChangeDetectionStrategy.OnPush,
	imports: [RouterLink]
})
export class HomeComponent {
	readonly Routes = Routes;
}
