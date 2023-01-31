import { SessionStore } from '@/stores/session-store';
import AuthenticationService from './authentication.service';

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

    const headers: HeadersInit = {
      'Content-Type': form
        ? 'application/x-www-form-urlencoded'
        : 'application/json',
    };
    headers['SameSite'] = 'Strict';

    if (SessionStore.token) {
      headers.Authorization = `bearer ${SessionStore.token}`;
    }

    fetchOptions.headers = { ...headers, ...options?.headers };

    const response = await fetch(url, fetchOptions);

    if (!response.ok) {
      // Note: Logging the user out on any API request returning 401 may not be the perfect solution,
      //       there could be a legitimate case for returning 401 that wouldn't require logout,
      //       but I don't think we have such a case at this point.
      if (response.status === 401 && SessionStore.isAuthenticated) {
        new AuthenticationService().logOut();
        window.location.href = '/';
      } else {
        const error = await response.json().catch(() => {
          console.debug('Unable to parse json response');
        });
        const errorMessage = error?.errorMessage ?? error;
        const defaultErrorMessage = `HTTP request error - ${response.status}: ${response.statusText}`;
        throw new Error(errorMessage ?? defaultErrorMessage);
      }
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
