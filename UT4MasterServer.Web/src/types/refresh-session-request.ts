import { GrantType } from "@/enums/grant-type";

export interface IRefreshSessionRequest {
    refresh_token: string;
    grant_type: GrantType;
}
