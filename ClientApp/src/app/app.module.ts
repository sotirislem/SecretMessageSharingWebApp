import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppRoutingModule } from './app-routing.module';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ToastrModule } from 'ngx-toastr';
import { AppInsightsModule } from '../insights/app-insights.module';

import { AppRootComponent } from './components/app-root/app-root.component';
import { NavMenuComponent } from './components/nav-menu/nav-menu.component';
import { LoaderComponent } from './components/loader/loader.component';
import { HomeComponent } from './components/home/home.component';
import { NewSecretMessageComponent } from './components/new-secret-message/new-secret-message.component';
import { GetSecretMessageComponent } from './components/get-secret-message/get-secret-message.component';
import { RecentlyStoredMessagesComponent } from './components/recent-stored-messages/recently-stored-messages.component';
import { AppInfoComponent } from './components/app-info/app-info.component';
import { MessageDeliveryDetailsModalComponent } from './components/message-delivery-details-modal/message-delivery-details-modal.component';

import { DateTimeFullPipe } from './pipes/date-time-full.pipe';

import { HttpLoaderInterceptor } from './interceptors/http-loader-interceptor.service';
import { HttpHeadersInterceptor } from './interceptors/http-headers-interceptor.service';
import { HttpErrorInterceptor } from './interceptors/http-error-interceptor.service';

import { environment } from '../environments/environment';


const appInsightsModule = (environment.production ? [AppInsightsModule] : []);

@NgModule({
	declarations: [
		AppRootComponent,
		NavMenuComponent,
		LoaderComponent,
		HomeComponent,
		NewSecretMessageComponent,
		GetSecretMessageComponent,
		RecentlyStoredMessagesComponent,
		AppInfoComponent,
		DateTimeFullPipe,
		MessageDeliveryDetailsModalComponent
	],
	imports: [
		BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
		AppRoutingModule,
		HttpClientModule,
		FormsModule,
		NgbModule,
		BrowserAnimationsModule,
		ToastrModule.forRoot(),
		...appInsightsModule
	],
	providers: [
		{ provide: HTTP_INTERCEPTORS, useClass: HttpLoaderInterceptor, multi: true },
		{ provide: HTTP_INTERCEPTORS, useClass: HttpHeadersInterceptor, multi: true },
		{ provide: HTTP_INTERCEPTORS, useClass: HttpErrorInterceptor, multi: true }
	],
	bootstrap: [AppRootComponent]
})
export class AppModule { }
