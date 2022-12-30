
import { defineConfig, UserConfig, loadEnv } from 'vite';
import vue from '@vitejs/plugin-vue';

export default defineConfig(({ command, mode }) => {
  const viteEnv = loadEnv(mode, process.cwd());
  const viteConfig = {
    define: {
      __UT4UU_API_URL: JSON.stringify(viteEnv.VITE_API_URL),
      __UT4UU_BASIC_AUTH: JSON.stringify(viteEnv.VITE_BASIC_AUTH)
    },
    plugins: [
      vue()
    ]
  } as UserConfig;

    if (command === 'serve') {
    viteConfig.server = {
      host: viteEnv.VITE_WEB_URL,
      port: 80
    };
  } else {
    viteConfig.base = '/dist/';
  }
  return viteConfig;
});
