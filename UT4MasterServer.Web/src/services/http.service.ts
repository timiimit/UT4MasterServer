import { SessionStore } from '@/stores/session-store';

type HttpMethod = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE';

export interface HttpRequestOptions<T = unknown> {
  body?: T;
  headers?: HeadersInit;
  formData?: FormData;
}

export class HttpError {
  code: string;
  message: string;

  constructor(code: string, message: string) {
    this.code = code;
    this.message = message;
  }
}

export default class HttpService {
  private formEncode(request: object) {
    const form = new URLSearchParams();
    for (const [key, value] of Object.entries(request)) {
      form.append(key, value);
    }

    return form;
  }

  async send<K = unknown, T = unknown>(
    url: string,
    options?: HttpRequestOptions<T>,
    method: HttpMethod = 'GET',
    form = true
  ): Promise<K> {
    const fetchOptions: RequestInit = { method };
    if (options?.body) {
      fetchOptions.body = form
        ? this.formEncode(options.body)
        : JSON.stringify(options.body);
    }

    if (options?.formData && form) {
      fetchOptions.body = options.formData;
    }

    const headers: HeadersInit = {};
    headers['SameSite'] = 'Strict';
    if (options?.body) {
      headers['Content-Type'] = form
        ? 'application/x-www-form-urlencoded'
        : 'application/json';
    }

    if (SessionStore.token) {
      headers.Authorization = `bearer ${SessionStore.token}`;
    }

    fetchOptions.headers = { ...headers, ...options?.headers };

    const response = await fetch(url, fetchOptions);

    if (!response.ok) {
      const errorResponseJson = await response
        .json()
        .catch(() => console.debug('Unable to parse error response JSON'));
      const errorResponseText = await response
        .text()
        .catch(() => console.debug('Unable to read error body.'));
      const errorMessage = errorResponseJson?.errorMessage ?? errorResponseText;
      const errorCode = errorResponseJson?.errorCode ?? response.status;
      const defaultErrorMessage = `HTTP request error - ${response.status}: ${response.statusText}`;
      throw new HttpError(errorCode, errorMessage ?? defaultErrorMessage);
    }

    const responseObj = await response.json().catch(() => {
      console.debug('Unable to parse json response');
    });

    return responseObj as K;
  }

  async get<K = unknown>(url: string, options?: HttpRequestOptions) {
    return this.send<K>(url, options, 'GET');
  }

  async post<K = unknown, T = unknown>(
    url: string,
    options?: HttpRequestOptions<T>,
    form = true
  ) {
    return this.send<K, T>(url, options, 'POST', form);
  }

  async put<K = unknown, T = unknown>(
    url: string,
    options?: HttpRequestOptions<T>,
    form = true
  ) {
    return this.send<K, T>(url, options, 'PUT', form);
  }

  async patch<K = unknown, T = unknown>(
    url: string,
    options?: HttpRequestOptions<T>,
    form = true
  ) {
    return this.send<K, T>(url, options, 'PATCH', form);
  }

  async delete<K = unknown, T = unknown>(
    url: string,
    options?: HttpRequestOptions<T>,
    form = true
  ) {
    return this.send<K, T>(url, options, 'DELETE', form);
  }
}
