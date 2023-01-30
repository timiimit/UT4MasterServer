import { createApp } from 'vue';
import App from './App.vue';
import { router } from './routes';
import { library } from '@fortawesome/fontawesome-svg-core';
import { faCopy } from '@fortawesome/free-regular-svg-icons';
import { faCheck } from '@fortawesome/free-solid-svg-icons';
import { faCertificate } from '@fortawesome/free-solid-svg-icons';

library.add(faCopy);
library.add(faCheck);
library.add(faCertificate);

createApp(App).use(router).mount('#app');
