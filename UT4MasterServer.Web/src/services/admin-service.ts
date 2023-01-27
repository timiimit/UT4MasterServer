import { ITrustedGameServer } from '@/types/trusted-game-server';
import HttpService from './http.service';

export default class AdminService extends HttpService {
    private baseUrl = `${__BACKEND_URL}/admin`;

    //#region Account Admin 

    async getAccountFlagOptions() {
        return await this.get<string[]>(`${this.baseUrl}/flags`);
    }

    async getFlagsForAccount(id: string) {
        return await this.get<string[]>(`${this.baseUrl}/flags/${id}`);
    }

    async setFlagsForAccount(id: string, flags: string[]) {
        return await this.put<string[]>(`${this.baseUrl}/flags/${id}`, { body: flags }, false);
    }

    //endregion

    async getTrustedServers() {
        return await this.get<ITrustedGameServer[]>(`${this.baseUrl}/trusted_servers`);
    }

    async updateTrustedServer(id: string, request: ITrustedGameServer) {
        return await this.patch(`${this.baseUrl}/trusted_servers/${id}`, { body: request });
    }

    async deleteTrustedServer(id: string) {
        return await this.delete(`${this.baseUrl}/trusted_servers/${id}`);
    }
}
