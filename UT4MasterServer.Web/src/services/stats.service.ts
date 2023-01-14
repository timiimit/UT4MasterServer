import { StatisticWindow } from '../enums/statistic-window';
import HttpService from './http.service';

export default class StatsService extends HttpService {
    async getStats(accountId: string, window: StatisticWindow) {
        return await this.get<unknown>(`${__BACKEND_URL}/ut/api/stats/accountId/${accountId}/bulk/window/${window}`);
    }
}
