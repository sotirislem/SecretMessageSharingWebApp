import { Component } from '@angular/core';
import { Routes } from '../../../constants';

@Component({
	selector: 'app-nav-menu',
	templateUrl: './nav-menu.component.html',
	styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent {
	readonly Routes = Routes;

	isExpanded: boolean;

	constructor() {
		this.isExpanded = false;
	}

	toggle() {
		this.isExpanded = !this.isExpanded;
	}

	collapse() {
		this.isExpanded = false;
	}
}
