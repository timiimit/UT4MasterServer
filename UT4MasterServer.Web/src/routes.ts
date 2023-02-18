import { createRouter, createWebHistory, RouteRecordRaw } from 'vue-router';
import { adminGuard, privateGuard, publicGuard } from './route-guards';

export const routes: RouteRecordRaw[] = [
  // Secure Pages
  {
    name: 'Stats',
    path: `/Stats/:window?/:accountId?`,
    component: async () => import('./pages/Stats/Stats.vue'),
    beforeEnter: privateGuard
  },
  {
    path: `/Profile`,
    component: async () => import('./pages/Profile/Profile.vue'),
    redirect: '/Profile/PlayerCard',
    beforeEnter: privateGuard,
    children: [
      {
        path: `PlayerCard`,
        component: async () => import('./pages/Profile/PlayerCard.vue'),
        beforeEnter: privateGuard
      },
      {
        path: `ChangeUsername`,
        component: async () => import('./pages/Profile/ChangeUsername.vue'),
        beforeEnter: privateGuard
      },
      {
        path: `ChangePassword`,
        component: async () => import('./pages/Profile/ChangePassword.vue'),
        beforeEnter: privateGuard
      },
      {
        path: `ChangeEmail`,
        component: async () => import('./pages/Profile/ChangeEmail.vue'),
        beforeEnter: privateGuard
      }
    ]
  },
  // Admin Pages
  {
    path: `/Admin`,
    children: [
      {
        path: `Accounts`,
        component: async () => import('./pages/Admin/Accounts/Accounts.vue'),
        beforeEnter: adminGuard
      },
      {
        path: `Clients`,
        component: async () => import('./pages/Admin/Clients/Clients.vue'),
        beforeEnter: adminGuard
      },
      {
        path: `TrustedServers`,
        component: async () =>
          import('./pages/Admin/TrustedServers/TrustedServers.vue'),
        beforeEnter: adminGuard
      },
      {
        path: `CloudFiles`,
        component: async () =>
          import('./pages/Admin/CloudFiles/CloudFiles.vue'),
        beforeEnter: adminGuard
      }
    ]
  },
  // Public Pages
  {
    path: `/Instructions`,
    children: [
      {
        path: `StockUT4`,
        component: async () => import('./pages/Instructions/StockUT4.vue')
      },
      {
        path: `UT4UU`,
        component: async () => import('./pages/Instructions/UT4UU.vue')
      },
      {
        path: `HubOwners`,
        component: async () => import('./pages/Instructions/HubOwners.vue')
      }
    ]
  },
  {
    path: `/Servers`,
    component: async () => import('./pages/Servers/Servers.vue')
  },
  {
    name: 'Rankings',
    path: `/Rankings/:type?/:page?`,
    component: async () => import('./pages/Rankings/Rankings.vue')
  },
  // Public Only Pages
  {
    path: `/Register`,
    component: async () => import('./pages/Register.vue'),
    beforeEnter: publicGuard
  },
  {
    path: `/Activation`,
    component: async () => import('./pages/Activation.vue'),
    beforeEnter: publicGuard
  },
  {
    name: 'Login',
    path: `/Login/:activationLinkSent?`,
    component: async () => import('./pages/Login.vue'),
    beforeEnter: publicGuard
  },
  {
    path: `/ForgotPassword`,
    component: async () => import('./pages/ForgotPassword.vue'),
    beforeEnter: publicGuard
  },
  {
    path: `/ResetPassword`,
    component: async () => import('./pages/ResetPassword.vue'),
    beforeEnter: publicGuard
  },
  {
    path: `/`,
    redirect: '/Login'
  },
  {
    path: '/:catchAll(.*)',
    component: async () => import('./pages/NotFound.vue')
  }
];

const router = createRouter({
  history: createWebHistory(),
  routes
});

export { router };
