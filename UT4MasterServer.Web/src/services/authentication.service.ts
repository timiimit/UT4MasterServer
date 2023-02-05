import { SessionStore as SessionStore } from '@/stores/session-store';
import { ILoginRequest } from '@/types/login-request';
import HttpService from './http.service';
import { ISession, IVerifySession } from '@/types/session';
import { GrantType } from '@/enums/grant-type';
import { IRefreshSessionRequest } from '@/types/refresh-session-request';
import { IAuthCodeResponse } from '@/types/auth-code-response';
import { AccountStore } from '@/stores/account-store';

export default class AuthenticationService extends HttpService {
  private baseUrl = `${__BACKEND_URL}/account/api/oauth`;

  async passwordLogin(request: ILoginRequest) {
    try {
      const session = await this.post<ISession, ILoginRequest>(
        `${this.baseUrl}/token`,
        {
          body: request,
          headers: {
            Authorization: `${__WEB_BASIC_AUTH}`
          }
        }
      );
      SessionStore.session = session;
    } catch (err: unknown) {
      SessionStore.session = null;
      throw err;
    }
  }

  async refreshSession(): Promise<boolean> {
    const tokenValid =
      SessionStore.session &&
      SessionStore.session.refresh_token &&
      new Date() < new Date(SessionStore.session.refresh_expires_at);
    if (!tokenValid || !SessionStore.session) {
      // Clear stored session if we have no valid refresh token to attempt session refresh
      SessionStore.session = null;
      return false;
    }
    try {
      const session = await this.post<ISession, IRefreshSessionRequest>(
        `${this.baseUrl}/token`,
        {
          body: {
            refresh_token: SessionStore.session.refresh_token,
            grant_type: GrantType.RefreshToken
          },
          headers: {
            Authorization: `${__WEB_BASIC_AUTH}`
          }
        }
      );
      SessionStore.session = session;
      return true;
    } catch (err: unknown) {
      SessionStore.session = null;
      return false;
    }
  }

  async checkAuth() {
    // No need to verify auth if we know we aren't authenticated
    if (!SessionStore.isAuthenticated || !SessionStore.session) {
      return false;
    }
    try {
      const verifySession = await this.get<IVerifySession>(
        `${this.baseUrl}/verify`
      );
      const session: ISession = {
        ...SessionStore.session,
        session_id: verifySession.session_id,
        access_token: verifySession.token,
        account_id: verifySession.account_id,
        displayName: verifySession.display_name,
        expires_at: verifySession.expires_at
      };
      SessionStore.session = session;
    } catch (err: unknown) {
      this.logOut();
      window.location.href = '/';
    }
    const tokenValid =
      SessionStore.session &&
      SessionStore.session.access_token &&
      new Date() < new Date(SessionStore.session.expires_at);
    if (tokenValid) {
      return true;
    }
    // If token expired try and refresh
    return await this.refreshSession();
  }

  async logOut() {
    try {
      if (SessionStore.session?.session_id) {
        await this.delete(
          `${this.baseUrl}/sessions/kill/${SessionStore.session.session_id}`
        );
      }
    } catch (err: unknown) {
      console.error('Error killing session', err);
    } finally {
      this.clearSession();
    }
  }

  async getAuthCode() {
    const authResponse = await this.get<IAuthCodeResponse>(
      `${this.baseUrl}/auth`
    );
    return authResponse.authorizationCode;
  }

  private clearSession() {
    AccountStore.account = null;
    SessionStore.session = null;
  }
}
