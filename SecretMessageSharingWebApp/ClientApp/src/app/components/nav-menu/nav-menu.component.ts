import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

import { Routes } from '../../../constants';

@Component({
	selector: 'app-nav-menu',
	templateUrl: './nav-menu.component.html',
	styleUrl: './nav-menu.component.css',
	changeDetection: ChangeDetectionStrategy.OnPush,
	imports: [RouterLink, RouterLinkActive]
})
export class NavMenuComponent {
	readonly Routes = Routes;
	readonly isExpanded = signal(false);

	toggle(): void {
		this.isExpanded.update(expanded => !expanded);
	}

	collapse(): void {
		this.isExpanded.set(false);
	}
}
