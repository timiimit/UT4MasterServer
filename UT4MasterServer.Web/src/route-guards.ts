import AuthenticationService from './services/authentication.service';
import { AccountStore } from './stores/account-store';

const authenticationService = new AuthenticationService();

export async function publicGuard() {
  if (await authenticationService.checkAuth()) {
    return { path: '/Profile' };
  }
}

export async function privateGuard() {
  // ensure that when entering a private page that the account is loaded
  if (AccountStore.account === null) {
    AccountStore.fetchUserAccount();
  }
  if (!(await authenticationService.checkAuth())) {
    return { path: '/Login' };
  }
}

export async function adminGuard() {
  if (AccountStore.account === null) {
    await AccountStore.fetchUserAccount();
  }
  return AccountStore.isAdmin;
}
