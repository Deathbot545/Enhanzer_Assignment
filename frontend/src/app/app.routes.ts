import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';
import { LoginComponent } from './pages/login/login.component';
import { PurchaseBillAddComponent } from './pages/purchase-bill-add/purchase-bill-add.component';

export const routes: Routes = [
	{ path: '', pathMatch: 'full', redirectTo: 'login' },
	{ path: 'login', component: LoginComponent },
	{
		path: 'purchase-bill-add',
		component: PurchaseBillAddComponent,
		canActivate: [authGuard]
	},
	{ path: '**', redirectTo: 'login' }
];
