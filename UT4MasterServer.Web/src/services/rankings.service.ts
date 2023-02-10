import { IPagedResponse } from '@/types/paged-response';
import { IRanking } from '@/types/ranking';
import HttpService from './http.service';

export class RankingsService extends HttpService {
  private baseUrl = `${__BACKEND_URL}/ut/api/game/v2/ratings`;

  async getRankings(
    ratingType: string,
    skip: number,
    limit: number
  ): Promise<IPagedResponse<IRanking>> {
    const response = await this.get<IPagedResponse<IRanking>>(
      `${this.baseUrl}/rankings?ratingType=${ratingType}&skip=${skip}&limit=${limit}`
    );
    const mapped: IPagedResponse<IRanking> = {
      count: response.count,
      data: response.data.map(this.mapRanking)
    };

    return mapped;
  }

  async getSelectedRanking(ratingType: string, accountId: string) {
    const response = await this.get<IRanking>(
      `${this.baseUrl}/ranking/${accountId}?ratingType=${ratingType}`
    );
    return this.mapRanking(response);
  }

  mapRanking(ranking: IRanking): IRanking {
    return {
      ...ranking,
      countryFlag: ranking.countryFlag?.replaceAll('.', ' ') ?? 'Unreal'
    };
  }
}
