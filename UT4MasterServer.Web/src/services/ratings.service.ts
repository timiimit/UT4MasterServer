import { IPagedResponse } from '@/types/paged-response';
import { IRanking } from '@/types/ranking';
import HttpService from './http.service';

export default class RatingsService extends HttpService {
  private baseUrl = `${__BACKEND_URL}/ut/api/game/v2/ratings`;

  async getRankings(ratingType: string, skip: number, limit: number) {
    return await this.get<IPagedResponse<IRanking>>(
      `${this.baseUrl}/rankings?ratingType=${ratingType}&skip=${skip}&limit=${limit}`
    );
  }
}
