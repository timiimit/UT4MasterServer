import { IAccount } from '../types/account';
import { IChangeEmailRequest } from '../types/change-email-request';
import { IChangePasswordRequest } from '../types/change-password-request';
import { IChangeUsernameRequest } from '../types/change-username-request';
import { IRegisterRequest } from '../types/register-request';
import HttpService from './http.service';

export default class AccountService extends HttpService {
    private baseUrl = `${__BACKEND_URL}/account/api`;

    async register(request: IRegisterRequest) {
        return await this.post<unknown, IRegisterRequest>(`${this.baseUrl}/create/account`, { body: request });
    }

    async changeUsername(request: IChangeUsernameRequest) {
        return await this.patch<unknown, IChangeUsernameRequest>(`${this.baseUrl}/update/username`, { body: request });
    }

    async changePassword(request: IChangePasswordRequest) {
        return await this.patch<unknown, IChangePasswordRequest>(`${this.baseUrl}/update/password`, { body: request });
    }

    async changeEmail(request: IChangeEmailRequest) {
        return await this.patch<unknown, IChangeEmailRequest>(`${this.baseUrl}/update/email`, { body: request });
    }

    async getAccount(id: string) {
        return await this.get<IAccount>(`${this.baseUrl}/public/account/${id}`);
    }
}
