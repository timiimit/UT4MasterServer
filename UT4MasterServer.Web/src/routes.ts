import {
  createRouter,
  createWebHistory,
  RouteRecordRaw
} from 'vue-router';
import { privateGuard, publicGuard } from './route-guards';

export const routes: RouteRecordRaw[] = [
  {
    path: `/Profile`,
    component: async () =>
      import(
        './pages/Profile/Profile.vue'
      ),
    redirect: '/Profile/Stats',
    beforeEnter: privateGuard,
    children: [
      {
        path: `ChangeUsername`,
        component: async () =>
          import(
            './pages/Profile/ChangeUsername.vue'
          ),
          beforeEnter: privateGuard
      },
      {
        path: `ChangePassword`,
        component: async () =>
          import(
            './pages/Profile/ChangePassword.vue'
          ),
          beforeEnter: privateGuard
      },
      {
        path: `ChangeEmail`,
        component: async () =>
          import(
            './pages/Profile/ChangeEmail.vue'
          ),
          beforeEnter: privateGuard
      },
      {
        path: `Stats`,
        component: async () =>
          import(
            './pages/Stats.vue'
          ),
          beforeEnter: privateGuard
      }
    ]
  },
  {
    path: `/Instructions`,
    children: [
      {
        path: `StockUT4`,
        component: async () =>
          import(
            './pages/Instructions/StockUT4.vue'
          )
      },
      {
        path: `UT4UU`,
        component: async () =>
          import(
            './pages/Instructions/UT4UU.vue'
          )
      },
      {
        path: `HubOwners`,
        component: async () =>
          import(
            './pages/Instructions/HubOwners.vue'
          )
      }
    ]
  },
  {
    path: `/Register`,
    component: async () =>
      import(
        './pages/Register.vue'
      ),
      beforeEnter: publicGuard
  },
  {
    path: `/Login`,
    component: async () =>
      import(
        './pages/Login.vue'
      ),
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
