import { ChangeDetectionStrategy, Component, inject } from '@angular/core';

import { LoaderService } from '../../services/loader.service';

@Component({
	selector: 'app-loader',
	templateUrl: './loader.component.html',
	styleUrl: './loader.component.css',
	changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoaderComponent {
	readonly loaderService = inject(LoaderService);
}
