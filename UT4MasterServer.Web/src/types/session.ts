export interface ISession {
	access_token: string;
	expires_at: string; //ISO date
	refresh_token: string;
	refresh_expires_at: string; //ISO date
	account_id: string;
	displayName: string;
	session_id: string;
}

export interface IVerifySession {
	token: string;
	session_id: string;
	display_name: string;
	expires_at: string; //ISO date
	account_id: string;
}
