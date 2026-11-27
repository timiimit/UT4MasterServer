export interface IAdminChangePasswordRequest {
	newPassword: string;
	email: string;
	iAmSure: boolean;
}
