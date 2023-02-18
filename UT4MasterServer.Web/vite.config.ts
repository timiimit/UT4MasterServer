import { defineConfig, UserConfig, loadEnv } from 'vite';
import vue from '@vitejs/plugin-vue';
import path from 'path';

export default defineConfig(({ command, mode }) => {
  const viteEnv = loadEnv(mode, process.cwd());
  const viteConfig = {
    define: {
      __BACKEND_URL: JSON.stringify(viteEnv.VITE_API_URL),
      __WEB_BASIC_AUTH: JSON.stringify(viteEnv.VITE_BASIC_AUTH),
      __RECAPTCHA_SITE_KEY: JSON.stringify(viteEnv.VITE_RECAPTCHA_SITE_KEY)
    },
    plugins: [vue()],
    resolve: {
      alias: {
        '@': path.resolve(__dirname, './src')
      }
    }
  } as UserConfig;

  if (mode === 'production') {
    if (!viteEnv.VITE_API_URL?.length) {
      throw new Error(
        'Missing environment variable VITE_API_URL required for production build.'
      );
    }
    if (!viteEnv.VITE_BASIC_AUTH?.length) {
      throw new Error(
        'Missing environment variable VITE_BASIC_AUTH required for production build.'
      );
    }
    if (!viteEnv.VITE_RECAPTCHA_SITE_KEY?.length) {
      throw new Error(
        'Missing environment variable VITE_RECAPTCHA_SITE_KEY required for production build.'
      );
    }
  }

  if (command === 'serve') {
    viteConfig.server = {
      port: 8080
    };
  } else {
    viteConfig.base = '/';
  }
  return viteConfig;
});
