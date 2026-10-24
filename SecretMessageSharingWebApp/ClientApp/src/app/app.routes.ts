import { Route } from '@angular/router';

import { Routes as RouteConstants } from '../constants';
import { getSecretMessageRouteGuard } from './guards/get-secret-message-route-guard';
import { newSecretMessageRouteGuard } from './guards/new-secret-message-route-guard';

export const AppRoutes: Route[] = [
	{
		path: '',
		pathMatch: 'full',
		title: 'Share Secrets Securely',
		loadComponent: () => import('./components/home/home.component').then(m => m.HomeComponent)
	},
	{
		path: RouteConstants.CreateSecretMessage,
		title: 'Create a Message',
		loadComponent: () => import('./components/create-secret-message/create-secret-message.component').then(m => m.CreateSecretMessageComponent)
	},
	{
		path: RouteConstants.NewSecretMessage,
		title: 'New Message',
		canActivate: [newSecretMessageRouteGuard],
		loadComponent: () => import('./components/new-secret-message/new-secret-message.component').then(m => m.NewSecretMessageComponent)
	},
	{
		path: RouteConstants.GetSecretMessage,
		title: 'Secret Message',
		canActivate: [getSecretMessageRouteGuard],
		loadComponent: () => import('./components/get-secret-message/get-secret-message.component').then(m => m.GetSecretMessageComponent)
	},
	{
		path: RouteConstants.RecentlyStoredMessages,
		title: 'Recently stored Messages',
		loadComponent: () => import('./components/recent-stored-messages/recently-stored-messages.component').then(m => m.RecentlyStoredMessagesComponent)
	},
	{
		path: RouteConstants.AppInfo,
		title: 'App Info',
		loadComponent: () => import('./components/app-info/app-info.component').then(m => m.AppInfoComponent)
	},
	{ path: '**', redirectTo: '' }
];
