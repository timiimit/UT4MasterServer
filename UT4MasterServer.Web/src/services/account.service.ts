import { IRegisterRequest } from '../types/register-request';
import HttpService from './http.service';

export default class AccountService extends HttpService {
    private registerUrl = `${__BACKEND_URL}/account/api/create/account`;

    async register(request: IRegisterRequest) {
        return await this.post<unknown, IRegisterRequest>(this.registerUrl, { body: request });
    }

    changeUsername() {

    }

    changePassword() {

    }
}
