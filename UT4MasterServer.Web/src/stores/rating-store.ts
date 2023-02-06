import { ref, shallowRef } from 'vue';
import { IRanking } from '@/types/rating';
import { AsyncStatus } from '@/types/async-status';
import RatingService from '@/services/ratings.service';

const _ratingService = new RatingService();
const _status = ref(AsyncStatus.OK);
const _rankings = ref<IRanking[]>([]);
const _rankingsCount = shallowRef(0);

export const RatingStore = {
  get rankings() {
    return _rankings.value;
  },
  get rankingsCount() {
    return _rankingsCount.value;
  },
  get status() {
    return _status.value;
  },
  async fetchRankings(ratingType: string, skip: number, limit: number) {
    try {
      _status.value = AsyncStatus.BUSY;

      let response = await _ratingService.getRankings(ratingType, skip, limit);
      _rankings.value = response.data;
      _rankingsCount.value = response.count;
      _status.value = AsyncStatus.OK;
    } catch (err: unknown) {
      console.error('Error occurred while fetching rankings:', err);
      _status.value = AsyncStatus.ERROR;
    }
  }
};
