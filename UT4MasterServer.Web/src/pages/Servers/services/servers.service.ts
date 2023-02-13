import HttpService from '@/services//http.service';
//import { getServersTestResponse } from '../data/get-servers-test-response';
import { IMatchmakingResponse } from '../types/matchmaking-response';

export default class ServerService extends HttpService {
  private baseUrl = `${__BACKEND_URL}/ut/api/matchmaking/session`;

  async getAllServers() {
    return await this.get<IMatchmakingResponse[]>(
      `${this.baseUrl}/matchMakingRequest`
    );
    //return getServersTestResponse;
  }
}
