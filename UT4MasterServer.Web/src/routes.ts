import {
  createRouter,
  createWebHistory,
  RouteLocationNormalized,
  RouteRecordRaw
} from 'vue-router';
import { UserStore } from './stores/user-store';

export const routes: RouteRecordRaw[] = [
  {
    path: `/Profile`,
    component: async () =>
      import(
        './pages/Profile.vue'
      ),
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

function publicGuard(to: RouteLocationNormalized) {
  if (UserStore.isAuthenticated) {
    return { path: '/Profile/Stats' };
  }
}

function privateGuard(to: RouteLocationNormalized) {
  if (!UserStore.isAuthenticated) {
    return { path: '/Login' };
  }
}

router.beforeEach((to) => {
  return to.meta.public ? publicGuard(to) : privateGuard(to);
});

export { router };
