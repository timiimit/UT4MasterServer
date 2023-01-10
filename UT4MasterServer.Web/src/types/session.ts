export interface ISession {
    "access_token": string,
    "expires_in": number,
    "expires_at": string, //ISO date
    "token_type": string, // TODO: make enum of possible values
    "refresh_token": string,
    "refresh_expires": number,
    "refresh_expires_at": string, //ISO date
    "account_id": string,
    "client_id": string,
    "internal_client": false,
    "client_service": string, // TODO: make enum of possible values
    "displayName": string,
    "app": string, // TODO: make enum of possible values
    "in_app_id": string,
    "device_id": string
}