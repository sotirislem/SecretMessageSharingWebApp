import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Routes as RouteConstants } from '../constants';

import { HomeComponent } from "./components/home/home.component";
import { AppInfoComponent } from './components/app-info/app-info.component';
import { NewSecretMessageComponent } from "./components/new-secret-message/new-secret-message.component";
import { GetSecretMessageComponent } from "./components/get-secret-message/get-secret-message.component";

import { NewSecretMessageRouteGuard } from './guards/new-secret-message-route-guard';
import { GetSecretMessageRouteGuard } from './guards/get-secret-message-route-guard';

const AppRoutes: Routes = [
	{ path: '', component: HomeComponent, pathMatch: 'full' },
	{ path: RouteConstants.Root_AppInfo, component: AppInfoComponent },
	{ path: RouteConstants.Root_NewSecretMessage, component: NewSecretMessageComponent, canActivate: [NewSecretMessageRouteGuard] },
	{ path: RouteConstants.Root_GetSecretMessage, component: GetSecretMessageComponent, canActivate: [GetSecretMessageRouteGuard] },
	{ path: '**', redirectTo:'' }
]

@NgModule({
	imports: [RouterModule.forRoot(AppRoutes)],
	exports: [RouterModule]
})
export class AppRoutingModule { }
