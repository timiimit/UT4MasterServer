import { Role } from '@/enums/role';
import { IAccount, IAccountExtended } from '@/types/account';
import { IChangeEmailRequest } from '@/types/change-email-request';
import { IChangePasswordRequest } from '@/types/change-password-request';
import { IChangeUsernameRequest } from '@/types/change-username-request';
import { IRegisterRequest } from '@/types/register-request';
import { ISearchAccountsResponse } from '@/types/search-accounts-response';
import { IResetPasswordRequest } from '@/types/reset-password';
import HttpService from './http.service';

export default class AccountService extends HttpService {
  private baseUrl = `${__BACKEND_URL}/account/api`;
  private personaBaseUrl = `${__BACKEND_URL}/persona/api`;

  async register(request: IRegisterRequest) {
    return await this.post<unknown, IRegisterRequest>(
      `${this.baseUrl}/create/account`,
      { body: request }
    );
  }

  async changeUsername(request: IChangeUsernameRequest) {
    return await this.patch<unknown, IChangeUsernameRequest>(
      `${this.baseUrl}/update/username`,
      { body: request }
    );
  }

  async changePassword(request: IChangePasswordRequest) {
    return await this.patch<unknown, IChangePasswordRequest>(
      `${this.baseUrl}/update/password`,
      { body: request }
    );
  }

  async changeEmail(request: IChangeEmailRequest) {
    return await this.patch<unknown, IChangeEmailRequest>(
      `${this.baseUrl}/update/email`,
      { body: request }
    );
  }

  async getAccount(id: string) {
    return await this.get<IAccountExtended>(
      `${this.personaBaseUrl}/account/${id}`
    );
  }

  async searchAccounts<T extends IAccount = IAccount>(
    query = '',
    skip = 0,
    take = 10,
    includeRoles = false,
    roles: Role[] | null = null
  ) {
    return await this.post<ISearchAccountsResponse<T>>(
      `${this.personaBaseUrl}/accounts/search`,
      { body: { query, skip, take, includeRoles, roles } },
      false
    );
  }

  async getAccountsByIds(ids: string[]) {
    return await this.post<IAccount[]>(
      `${this.personaBaseUrl}/accounts`,
      { body: ids },
      false
    );
  }

  async activateAccount(email: string, guid: string) {
    return await this.get<boolean>(
      `${this.baseUrl}/activate?email=${email}&guid=${guid}`
    );
  }

  async initiateResetPassword(email: string) {
    return await this.get<string>(
      `${this.baseUrl}/initiate-reset-password?email=${email}`
    );
  }

  async resetPassword(request: IResetPasswordRequest) {
    return await this.post<unknown, IResetPasswordRequest>(
      `${this.baseUrl}/reset-password`,
      {
        body: request
      }
    );
  }
}
