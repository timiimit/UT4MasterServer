
import { defineConfig, UserConfig, loadEnv } from 'vite';
import vue from '@vitejs/plugin-vue';

export default defineConfig(({ command, mode }) => {
  const viteEnv = loadEnv(mode, process.cwd());
  const viteConfig = {
    define: {
      __BACKEND_URL: JSON.stringify(viteEnv.VITE_API_URL),
      __WEB_BASIC_AUTH: JSON.stringify(viteEnv.VITE_BASIC_AUTH)
    },
    plugins: [
      vue()
    ]
  } as UserConfig;

  if (command === 'serve') {
    viteConfig.server = {
      port: 8080
    };
  } else {
    viteConfig.base = '/dist/';
  }
  return viteConfig;
});
