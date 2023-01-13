import {
  createRouter,
  createWebHistory,
  RouteRecordRaw
} from 'vue-router';
import { beforeEachGuard } from './route-guards';

export const routes: RouteRecordRaw[] = [
  {
    path: `/Profile`,
    component: async () =>
      import(
        './pages/Profile.vue'
      ),
    redirect: '/Profile/Stats',
    children: [
      {
        path: `ChangeUsername`,
        component: async () =>
          import(
            './pages/ChangeUsername.vue'
          )
      },
      {
        path: `ChangePassword`,
        component: async () =>
          import(
            './pages/ChangePassword.vue'
          )
      },
      {
        path: `ChangeEmail`,
        component: async () =>
          import(
            './pages/ChangeEmail.vue'
          )
      },
      {
        path: `Stats`,
        component: async () =>
          import(
            './pages/Stats.vue'
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
    meta: {
      public: true
    }
  },
  {
    path: `/Login`,
    component: async () =>
      import(
        './pages/Login.vue'
      ),
    meta: {
      public: true
    }
  },
  {
    path: `/`,
    redirect: '/Login',
    meta: {
      public: true
    }
  },
  {
    path: '/:catchAll(.*)',
    component: async () => import('./pages/NotFound.vue'),
    meta: {
      public: true
    }
  }
];

const router = createRouter({
  history: createWebHistory(),
  routes
});

router.beforeEach(beforeEachGuard);

export { router };
