export interface IAuthCodeResponse {
    authorizationCode: string;
    redirectUrl: string;
    sid: string | null;
}
