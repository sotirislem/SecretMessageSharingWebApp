import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { AppRoutingModule } from './app-routing.module';

import { AppRootComponent } from './components/app-root/app-root.component';
import { NavMenuComponent } from './components/nav-menu/nav-menu.component';
import { LoaderComponent } from './components/loader/loader.component';
import { HomeComponent } from './components/home/home.component';
import { NewSecretMessageComponent } from './components/new-secret-message/new-secret-message.component';
import { GetSecretMessageComponent } from './components/get-secret-message/get-secret-message.component';
import { AppInfoComponent } from './components/app-info/app-info.component';

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ToastrModule } from 'ngx-toastr';

import { HttpLoaderInterceptor } from './interceptors/http-loader-interceptor.service';
import { HttpHeadersInterceptor } from './interceptors/http-headers-interceptor.service';
import { HttpErrorInterceptor } from './interceptors/http-error-interceptor.service';

@NgModule({
	declarations: [
		AppRootComponent,
		NavMenuComponent,
		LoaderComponent,
		HomeComponent,
		NewSecretMessageComponent,
		GetSecretMessageComponent,
		AppInfoComponent
	],
	imports: [
		BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
		AppRoutingModule,
		HttpClientModule,
		FormsModule,
		NgbModule,
		BrowserAnimationsModule,
		ToastrModule.forRoot()
	],
	providers: [
		{ provide: HTTP_INTERCEPTORS, useClass: HttpLoaderInterceptor, multi: true },
		{ provide: HTTP_INTERCEPTORS, useClass: HttpHeadersInterceptor, multi: true },
		{ provide: HTTP_INTERCEPTORS, useClass: HttpErrorInterceptor, multi: true }
	],
	bootstrap: [AppRootComponent]
})
export class AppModule { }
