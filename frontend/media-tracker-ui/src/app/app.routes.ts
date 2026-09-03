import { Routes } from '@angular/router';
import { AppShell } from './app-shell/app-shell';
import { Blank } from './features/blank/blank';

export const routes: Routes = [
    {
        path: '',
        component: AppShell,
        children: [
            { path: 'items', component: Blank},
            { path: 'blank', component: Blank },
            { path: '', redirectTo: '/items', pathMatch: 'full' }
        ]
    }
];
