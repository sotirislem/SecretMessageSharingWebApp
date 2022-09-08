import { Component } from '@angular/core';
import { Routes } from '../../../constants';
@Component({
	selector: 'app-home',
	templateUrl: './home.component.html',
	styleUrls: ['./home.component.css']
})
export class HomeComponent {
	readonly Routes = Routes;

	constructor() { }
}
