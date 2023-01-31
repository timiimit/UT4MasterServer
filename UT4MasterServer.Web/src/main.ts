import { createApp } from 'vue';
import App from './App.vue';
import { router } from './routes';
import { library } from '@fortawesome/fontawesome-svg-core';
import { faCopy } from '@fortawesome/free-regular-svg-icons';
import { faCheck } from '@fortawesome/free-solid-svg-icons';
import { faCertificate } from '@fortawesome/free-solid-svg-icons';
import { faPenToSquare } from '@fortawesome/free-regular-svg-icons';
import { faTrashCan } from '@fortawesome/free-solid-svg-icons';
import { faPlus } from '@fortawesome/free-solid-svg-icons';
import { faChevronUp } from '@fortawesome/free-solid-svg-icons';
import { faChevronDown } from '@fortawesome/free-solid-svg-icons';
import { faChevronLeft } from '@fortawesome/free-solid-svg-icons';
import { faChevronRight } from '@fortawesome/free-solid-svg-icons';

library.add(faCopy);
library.add(faCheck);
library.add(faCertificate);
library.add(faPenToSquare);
library.add(faTrashCan);
library.add(faPlus);
library.add(faChevronUp);
library.add(faChevronDown);
library.add(faChevronLeft);
library.add(faChevronRight);

createApp(App).use(router).mount('#app');
