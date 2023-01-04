import { UserStore } from "../stores/user-store";

type HttpMethod = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE';

export interface HttpRequestOptions<T = unknown> {
    body?: T;
    headers?: HeadersInit;
}

export default class HttpService {
    async send<K = unknown, T = unknown>(url: string, options?: HttpRequestOptions<T>, method: HttpMethod = 'GET'): Promise<K> {
        const fetchOptions: RequestInit = { method };
        if (options?.body) {
            fetchOptions.body = JSON.stringify(options.body);
        }

        const headers: HeadersInit = { 'Content-Type': 'application/json' };

        if (UserStore.authToken) {
            headers.Authorization = `bearer ${UserStore.authToken}`;
        }

        fetchOptions.headers = { ...headers, ...options?.headers };

        const response = await fetch(url, fetchOptions);

        if (!response.ok) {
            throw new Error(`HTTP request error - ${response.status}: ${response.statusText}`);
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
