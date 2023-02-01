import { IGameServer } from 'src/types/game-server';
import HttpService from './http.service';

export default class ServerService extends HttpService {
  private baseUrl = `${__BACKEND_URL}/ut/api/matchmaking/session`;

  async getGameServers() {
    return await this.get<IGameServer[]>(`${this.baseUrl}/matchMakingRequest`);
  }
}
