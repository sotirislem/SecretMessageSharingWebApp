import { Component } from '@angular/core';
import { LoaderService } from '../../services/loader.service';

@Component({
	selector: 'app-loader',
	templateUrl: './loader.component.html',
	styleUrls: ['./loader.component.css']
})
export class LoaderComponent {
	loaderActive: boolean;

	constructor(private loaderService: LoaderService) {
		this.loaderService.loadingObservable$.subscribe((isLoading) => {
			this.loaderActive = isLoading;
		});
	}
}
