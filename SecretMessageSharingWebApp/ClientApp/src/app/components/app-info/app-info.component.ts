import { ChangeDetectionStrategy, Component } from '@angular/core';

import { NgbAccordionModule } from '@ng-bootstrap/ng-bootstrap';

@Component({
	templateUrl: './app-info.component.html',
	styleUrl: './app-info.component.css',
	changeDetection: ChangeDetectionStrategy.OnPush,
	imports: [NgbAccordionModule]
})
export class AppInfoComponent {
}
