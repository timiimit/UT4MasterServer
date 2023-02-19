export interface IRegisterRequest {
  username: string;
  password: string;
  email: string;
  recaptchaToken?: string;
}
