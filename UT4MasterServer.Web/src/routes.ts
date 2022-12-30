import {
    createRouter,
    createWebHistory,
    RouteRecordRaw
  } from 'vue-router';
  
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
  
  router.beforeEach((to) => {
    return to.meta.public === true;
  });
  
  export { router };
  