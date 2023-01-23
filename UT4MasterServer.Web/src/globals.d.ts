import { IGlobalPropertyFilters } from '@shared/interfaces';

// Yep this is necessary
export {};

declare global {
	const __BACKEND_URL: string;
	const __WEB_BASIC_AUTH: string;
	const __RECAPTCHA_SITE_KEY: string;
}
