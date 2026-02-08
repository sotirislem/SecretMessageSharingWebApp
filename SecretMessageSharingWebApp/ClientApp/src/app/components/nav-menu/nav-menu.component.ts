import { Component } from '@angular/core';
import { NgClass } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Routes } from '../../../constants';

@Component({
	selector: 'app-nav-menu',
	templateUrl: './nav-menu.component.html',
	styleUrls: ['./nav-menu.component.css'],
	standalone: true,
	imports: [NgClass, RouterModule]
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
