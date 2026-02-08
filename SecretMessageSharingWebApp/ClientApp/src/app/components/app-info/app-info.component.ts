import { Component } from '@angular/core';

import { NgbAccordionModule } from '@ng-bootstrap/ng-bootstrap';

@Component({
	templateUrl: './app-info.component.html',
	styleUrls: ['./app-info.component.css'],
	standalone: true,
	imports: [NgbAccordionModule]
})
export class AppInfoComponent {

	constructor() { }

}
