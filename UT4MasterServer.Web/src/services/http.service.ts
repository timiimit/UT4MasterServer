import { SessionStore } from "../stores/session-store";

type HttpMethod = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE';

export interface HttpRequestOptions<T = unknown> {
    body?: T;
    headers?: HeadersInit;
}

export default class HttpService {

    private formEncode(request: object) {
        const form = new URLSearchParams();
        for (const [key, value] of Object.entries(request)) {
            form.append(key, value);
        }

        return form;
    }

    async send<K = unknown, T = unknown>(url: string, options?: HttpRequestOptions<T>, method: HttpMethod = 'GET'): Promise<K> {
        const fetchOptions: RequestInit = { method };
        if (options?.body) {
            fetchOptions.body = this.formEncode(options.body);
        }

        const headers: HeadersInit = { 'Content-Type': 'application/x-www-form-urlencoded' };

        if (SessionStore.token) {
            headers.Authorization = `bearer ${SessionStore.token}`;
        }

        fetchOptions.headers = { ...headers, ...options?.headers };

        const response = await fetch(url, fetchOptions);

        if (!response.ok) {
            const error =  await response.json().catch(() => { });
            const errorMessage = error.errorMessage ?? error;
            const defaultErrorMessage = `HTTP request error - ${response.status}: ${response.statusText}`;
            throw new Error(errorMessage ?? defaultErrorMessage);
        }

        const responseObj = await response.json().catch(() => { });

        return responseObj as K;
    }

    async get<K = unknown>(url: string, options?: HttpRequestOptions) {
        return this.send<K>(url, options, 'GET');
    }

    async post<K = unknown, T = unknown>(url: string, options?: HttpRequestOptions<T>) {
        return this.send<K, T>(url, options, 'POST');
    }

    async put<K = unknown, T = unknown>(url: string, options?: HttpRequestOptions<T>) {
        return this.send<K, T>(url, options, 'PUT');
    }

    async patch<K = unknown, T = unknown>(url: string, options?: HttpRequestOptions<T>) {
        return this.send<K, T>(url, options, 'PATCH');
    }

    async delete<K = unknown, T = unknown>(url: string, options?: HttpRequestOptions<T>) {
        return this.send<K, T>(url, options, 'DELETE');
    }
}
