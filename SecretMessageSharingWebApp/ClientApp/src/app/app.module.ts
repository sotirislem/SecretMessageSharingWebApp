import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppRoutingModule } from './app-routing.module';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ToastrModule } from 'ngx-toastr';
import { DigitOnlyModule } from '@uiowa/digit-only';
import { AppInsightsModule } from '../insights/app-insights.module';

import { AppRootComponent } from './components/app-root/app-root.component';
import { NavMenuComponent } from './components/nav-menu/nav-menu.component';
import { LoaderComponent } from './components/loader/loader.component';
import { HomeComponent } from './components/home/home.component';
import { CreateSecretMessageComponent } from './components/create-secret-message/create-secret-message.component';
import { NewSecretMessageComponent } from './components/new-secret-message/new-secret-message.component';
import { GetSecretMessageComponent } from './components/get-secret-message/get-secret-message.component';
import { RecentlyStoredMessagesComponent } from './components/recent-stored-messages/recently-stored-messages.component';
import { AppInfoComponent } from './components/app-info/app-info.component';
import { MessageDeliveryDetailsModalComponent } from './components/modals/message-delivery-details-modal/message-delivery-details-modal.component';
import { OtpInputModalComponent } from './components/modals/otp-input-modal/otp-input-modal.component';
import { GenericConfirmationModalComponent } from './components/modals/generic-confirmation-modal/generic-confirmation-modal.component';

import { DateTimeFullPipe } from './pipes/date-time-full.pipe';

import { HttpLoaderInterceptor } from './interceptors/http-loader-interceptor.service';
import { HttpHeadersInterceptor } from './interceptors/http-headers-interceptor.service';
import { HttpErrorInterceptor } from './interceptors/http-error-interceptor.service';

import { environment } from '../environments/environment';

const appInsightsModule = (environment.production && environment.applicationInsights.enable ? [AppInsightsModule] : []);

@NgModule({
	declarations: [
		AppRootComponent,
		NavMenuComponent,
		LoaderComponent,
		HomeComponent,
		CreateSecretMessageComponent,
		NewSecretMessageComponent,
		GetSecretMessageComponent,
		RecentlyStoredMessagesComponent,
		AppInfoComponent,
		DateTimeFullPipe,
		MessageDeliveryDetailsModalComponent,
		OtpInputModalComponent,
		GenericConfirmationModalComponent
	],
	imports: [
		BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
		AppRoutingModule,
		HttpClientModule,
		FormsModule,
		ReactiveFormsModule,
		NgbModule,
		BrowserAnimationsModule,
		ToastrModule.forRoot(),
		DigitOnlyModule,
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
