import { Component } from '@angular/core';
import { LoaderService } from '../../services/loader.service';
import { Observable } from 'rxjs';
import { AsyncPipe } from '@angular/common';

@Component({
	selector: 'app-loader',
	templateUrl: './loader.component.html',
	styleUrls: ['./loader.component.css'],
	standalone: true,
	imports: [AsyncPipe]
})
export class LoaderComponent {
	loaderActive$: Observable<boolean>;

	constructor(private loaderService: LoaderService) {
		this.loaderActive$ = this.loaderService.loadingObservable$;
	}
}
