import { RouteLocationNormalized } from "vue-router";
import AuthenticationService from "./services/authentication.service";

const authenticationService = new AuthenticationService();

async function publicGuard(to: RouteLocationNormalized, authenticated: boolean) {
  if (authenticated) {
    return { path: '/Profile' };
  }
}

function privateGuard(to: RouteLocationNormalized, authenticated: boolean) {
  if (!authenticated) {
    return { path: '/Login' };
  }
}
export async function beforeEachGuard(to: RouteLocationNormalized) {
  const authenticated = await authenticationService.checkAuth();
  return to.meta.public ? publicGuard(to, authenticated) : privateGuard(to, authenticated);
};
