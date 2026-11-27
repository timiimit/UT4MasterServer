import { createApp } from 'vue';
import App from './App.vue';
import { router } from './routes';
import { library } from '@fortawesome/fontawesome-svg-core';
import { faCopy, faPenToSquare } from '@fortawesome/free-regular-svg-icons';
import { faGithub, faDiscord } from '@fortawesome/free-brands-svg-icons';
import {
	faCheck,
	faCertificate,
	faTrashCan,
	faPlus,
	faChevronUp,
	faChevronDown,
	faChevronLeft,
	faChevronRight,
	faArrowsRotate,
	faShare,
	faU
} from '@fortawesome/free-solid-svg-icons';

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
library.add(faGithub);
library.add(faDiscord);
library.add(faArrowsRotate);
library.add(faShare);
library.add(faU);

createApp(App).use(router).mount('#app');
