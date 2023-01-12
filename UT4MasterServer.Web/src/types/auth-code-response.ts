export interface IAuthCodeResponse {
    authorizationCode: string | null;
    redirectUrl: string | null;
    sid: string | null;
}
