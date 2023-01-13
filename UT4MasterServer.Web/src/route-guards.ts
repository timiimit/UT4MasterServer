import { RouteLocationNormalized } from "vue-router";
import AuthenticationService from "./services/authentication.service";

const authenticationService = new AuthenticationService();

export async function publicGuard(to: RouteLocationNormalized) {
  if (await authenticationService.checkAuth()) {
    return { path: '/Profile' };
  }
}

export async function privateGuard(to: RouteLocationNormalized) {
  if (!await authenticationService.checkAuth()) {
    return { path: '/Login' };
  }
}
