import { InjectionToken } from '@angular/core';

export const BASE_URL = new InjectionToken<string>('BASE_URL');
export const API_URL = new InjectionToken<string>('API_URL');
export const CLIENT_ID = new InjectionToken<string>('CLIENT_ID');

/**
 * Data handed to a component that is opened through `ModalService`.
 *
 * ng-bootstrap creates modal components itself, so signal inputs cannot be set
 * from the outside. A dedicated injector is used instead.
 */
export const MODAL_DATA = new InjectionToken<unknown>('MODAL_DATA');
