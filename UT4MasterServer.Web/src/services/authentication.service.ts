import { ILoginRequest } from '../types/login-request';

export default class AuthenticationService {
    private tokenUrl = `${__UT4UU_API_URL}/account/api/oauth/token`;
    private authUrl = `${__UT4UU_API_URL}/account/api/oauth/auth`;

    async logIn(request: ILoginRequest) {
        const sessionResult = await fetch(this.tokenUrl,
            {
                method: 'POST',
                body: JSON.stringify(request),
                headers: { 'Authorization': __UT4UU_BASIC_AUTH }
            });

        if (!sessionResult.ok) {
            throw new Error('Failed to get token');
        }

        const session = await sessionResult.json();
        console.debug('Token response JSON: ', session);
        const token = session['access_token'] ;
        const authResult = await fetch(this.authUrl,
            {
                method: 'GET',
                headers: { 'Authorization': `bearer ${token}` }
            });

        if (!authResult.ok) {
            throw new Error('Failed to authenticate token');
        }
        const authz = await authResult.json();
        console.debug('Auth response JSON: ', session);
        localStorage.setItem('ut4uu_authorizationCode', authz['authorizationCode']);
        //TODO: auth code expiry
        return authz['authorizationCode'];
    }
}
