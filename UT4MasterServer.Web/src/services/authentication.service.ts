import { IRegisterRequest } from 'src/types/register-request';
import { ILoginRequest } from '../types/login-request';
import HttpService from './http.service';

interface ILoginResponse {
    access_token: string;
}

interface IAuthResponse {
    authorizationCode: string;
}

export default class AuthenticationService extends HttpService {
    private tokenUrl = `${__UT4UU_API_URL}/account/api/oauth/token`;
    private authUrl = `${__UT4UU_API_URL}/account/api/oauth/auth`;
    private registerUrl = `${__UT4UU_API_URL}/account/api/create/account`;

    async logIn(request: ILoginRequest) {
        const session = await this.post<ILoginResponse, ILoginRequest>(this.tokenUrl, {
            body: request,
            headers: {
                'Authorization': 'basic MzRhMDJjZjhmNDQxNGUyOWIxNTkyMTg3NmRhMzZmOWE6ZGFhZmJjY2M3Mzc3NDUwMzlkZmZlNTNkOTRmYzc2Y2Y='
            }
        });
        console.debug('Token response JSON: ', session);
        const token = session.access_token;

        const authResponse = await this.get<IAuthResponse>(this.authUrl, { headers: { 'Authorization': `bearer ${token}` } });

        console.debug('Auth response JSON: ', authResponse);
        localStorage.setItem('ut4uu_authorizationCode', authResponse.authorizationCode);

        return authResponse.authorizationCode;
    }

    async register(request: IRegisterRequest) {
        return await this.post<unknown, IRegisterRequest>(this.registerUrl, { body: request });
    }
}
