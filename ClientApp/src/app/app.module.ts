import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { AppRoutingModule } from './app-routing.module';

import { AppRootComponent } from './components/app-root/app-root.component';
import { NavMenuComponent } from './components/nav-menu/nav-menu.component';
import { LoaderComponent } from './components/loader/loader.component';
import { LoaderInterceptor } from './interceptors/http-interceptor.service';
import { HomeComponent } from './components/home/home.component';
import { SaveSecretMessageComponent } from './components/save-secret-message/save-secret-message.component';
import { GetSecretMessageComponent } from './components/get-secret-message/get-secret-message.component';


@NgModule({
	declarations: [
		AppRootComponent,
		NavMenuComponent,
		LoaderComponent,
		HomeComponent,
		SaveSecretMessageComponent,
		GetSecretMessageComponent
	],
	imports: [
		BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
		AppRoutingModule,
		HttpClientModule,
		FormsModule,
		NgbModule
	],
	providers: [
		{ provide: HTTP_INTERCEPTORS, useClass: LoaderInterceptor, multi: true }
	],
	bootstrap: [AppRootComponent]
})
export class AppModule { }
