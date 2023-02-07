<template>
  <LoadingPanel :status="status">
    <div class="d-flex justify-content-center">
      <div class="col-md-6 col-sm-12 mb-4">
        <label for="ratingType" class="col-md-6 col-sm-12 col-form-label">
          Rating Type
        </label>
        <select
          v-model="ratingType"
          class="form-select"
          @change="handleParameterChange"
        >
          <option
            v-for="rankingType in ratingTypeOptions"
            :key="rankingType.value"
            :value="rankingType.value"
          >
            {{ rankingType.text }}
          </option>
        </select>
      </div>
    </div>
    <div class="d-flex justify-content-center">
      <div class="col-md-9 col-sm-12">
        <table v-if="rankings.length" class="table table-hover table-sm">
          <thead>
            <tr>
              <th>Rank</th>
              <th>Flag</th>
              <th>Player</th>
              <th>Rating</th>
              <th>Games Played</th>
            </tr>
          </thead>
          <tbody>
            <template v-for="ranking in rankings" :key="ranking.accountID">
              <tr
                :class="`${
                  AccountStore.account?.id === ranking.accountID
                    ? 'table-info'
                    : ''
                }`"
              >
                <td>{{ ranking.rank }}</td>
                <td>
                  <img
                    class="flag"
                    :src="`/assets/flags/${ranking.countryFlag}.png`"
                  />
                </td>
                <td>
                  <router-link :to="`/Stats/${ranking.accountID}`">
                    {{ ranking.player }}</router-link
                  >
                </td>
                <td>{{ ranking.rating }}</td>
                <td>{{ ranking.gamesPlayed }}</td>
              </tr>
            </template>
          </tbody>
        </table>
        <p v-if="!rankings.length" class="text-center">
          No data for this rating type
        </p>
      </div>
    </div>
    <div class="d-flex justify-content-center">
      <div class="col-md-9 col-sm-12">
        <Paging
          :key="ratingType"
          :item-count="rankingsCount"
          :page-size="pageSize"
          @update="handlePagingUpdate"
        />
      </div>
    </div>
  </LoadingPanel>
</template>

<script lang="ts" setup>
import { onMounted, shallowRef, watch } from 'vue';
import { IRanking } from '@/types/rating';
import { RatingType } from '@/enums/rating-type';
import { AccountStore } from '@/stores/account-store';
import { SessionStore } from '@/stores/session-store';
import { usePaging } from '@/hooks/use-paging.hook';
import LoadingPanel from '@/components/LoadingPanel.vue';
import Paging from '@/components/Paging.vue';
import RatingsService from '@/services/ratings.service';
import { AsyncStatus } from '@/types/async-status';

const { pageSize, pageStart, pageEnd, handlePagingUpdate } = usePaging(5);

const ratingType = shallowRef(RatingType.DMSkillRating);
const status = shallowRef(AsyncStatus.OK);
const rankings = shallowRef<IRanking[]>([]);
const rankingsCount = shallowRef(0);

const ratingService = new RatingsService();

const ratingTypeOptions = [
  { text: 'Deathmatch', value: RatingType.DMSkillRating },
  { text: 'Duel', value: RatingType.SkillRating },
  { text: 'Team Deathmatch', value: RatingType.TDMSkillRating },
  { text: 'Capture the Flag', value: RatingType.CTFSkillRating },
  { text: 'Showdown', value: RatingType.ShowdownSkillRating },
  { text: 'Flag Run', value: RatingType.FlagRunSkillRating }
  // Ranked gametype rankings not available at this time
  // { text: 'Ranked Duel', value: RatingType.RankedDuelSkillRating },
  // { text: 'Ranked Capture the Flag', value: RatingType.RankedCTFSkillRating },
  // { text: 'Ranked Showdown', value: RatingType.RankedShowdownSkillRating },
  // { text: 'Ranked Flag Run', value: RatingType.RankedFlagRunSkillRating }
];

async function loadRankings() {
  if (!ratingType.value) {
    return;
  }

  try {
    status.value = AsyncStatus.BUSY;
    const response = await ratingService.getRankings(
      ratingType.value,
      pageStart.value,
      pageSize
    );
    rankings.value = response.data;
    rankingsCount.value = response.count;
    status.value = AsyncStatus.OK;
  } catch (err: unknown) {
    status.value = AsyncStatus.ERROR;
    console.error(err);
  }
}

onMounted(() => {
  if (SessionStore.isAuthenticated) {
    if (AccountStore.account === null) {
      AccountStore.fetchUserAccount();
    }
  }
  loadRankings();
});

function handleParameterChange() {
  if (!ratingType.value) {
    return;
  }
  pageStart.value = 0;
  loadRankings();
}

watch(pageEnd, loadRankings);
</script>
