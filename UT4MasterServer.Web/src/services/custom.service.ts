import { IPlayerCard } from '../types/player-card';
import HttpService from './http.service';

export default class CustomService extends HttpService {
    private baseUrl = `${__BACKEND_URL}/api`;

    async getPlayerCard(id: string) {
        return await this.get<IPlayerCard>(`${this.baseUrl}/playerCard/${id}`);
    }
}
