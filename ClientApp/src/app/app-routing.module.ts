import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SaveSecretMessageComponent } from "./components/save-secret-message/save-secret-message.component";
import { GetSecretMessageComponent } from "./components/get-secret-message/get-secret-message.component";
import { HomeComponent } from "./components/home/home.component";
import { CanActivateSaveSecretMessageRouteGuard } from './guards/can-activate-save-secret-message-route';
import { CanActivateGetSecretMessageRouteGuard } from './guards/can-activate-get-secret-message-route';
import { Routes as RouteConstants } from '../constants';
import { AppInfoComponent } from './components/app-info/app-info.component';

const AppRoutes: Routes = [
	{ path: '', component: HomeComponent, pathMatch: 'full' },
	{ path: RouteConstants.Root_AppInfo, component: AppInfoComponent },
	{ path: RouteConstants.Root_SaveSecretMessage, component: SaveSecretMessageComponent, canActivate: [CanActivateSaveSecretMessageRouteGuard] },
	{ path: RouteConstants.Root_GetSecretMessage, component: GetSecretMessageComponent, canActivate: [CanActivateGetSecretMessageRouteGuard] },
	{ path: '**', redirectTo:''}
]

@NgModule({
	imports: [RouterModule.forRoot(AppRoutes)],
	exports: [RouterModule]
})
export class AppRoutingModule { }
