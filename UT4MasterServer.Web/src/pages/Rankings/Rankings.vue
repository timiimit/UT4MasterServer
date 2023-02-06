<template>
  <LoadingPanel :status="RatingStore.status">
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
              <th>Player</th>
              <th>Rating</th>
              <th>Games Played</th>
            </tr>
          </thead>
          <tbody>
            <template v-for="ranking in rankings" :key="ranking.accountId">
              <tr>
                <td>{{ ranking.rank }}</td>
                <td>{{ ranking.player }}</td>
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
import { RatingStore } from '@/stores/rating-store';
import { usePaging } from '@/hooks/use-paging.hook';
import LoadingPanel from '@/components/LoadingPanel.vue';
import Paging from '@/components/Paging.vue';

const { pageSize, pageStart, pageEnd, handlePagingUpdate } = usePaging(10);

const ratingType = shallowRef(RatingType.DMSkillRating);
const rankings = shallowRef<IRanking[]>([]);
const rankingsCount = shallowRef(0);

const ratingTypeOptions = [
  { text: 'Deathmatch', value: RatingType.DMSkillRating },
  { text: 'Duel', value: RatingType.SkillRating },
  { text: 'Team Deathmatch', value: RatingType.TDMSkillRating },
  { text: 'Capture the Flag', value: RatingType.CTFSkillRating },
  { text: 'Showdown', value: RatingType.ShowdownSkillRating },
  { text: 'Flag Run', value: RatingType.FlagRunSkillRating },
  { text: 'Ranked Duel', value: RatingType.RankedDuelSkillRating },
  { text: 'Ranked Capture the Flag', value: RatingType.RankedCTFSkillRating },
  { text: 'Ranked Showdown', value: RatingType.RankedShowdownSkillRating },
  { text: 'Ranked Flag Run', value: RatingType.RankedFlagRunSkillRating }
];

async function loadRankings() {
  if (!ratingType.value) return;

  try {
    await RatingStore.fetchRankings(
      ratingType.value,
      pageStart.value,
      pageSize
    );
    rankings.value = RatingStore.rankings;
    rankingsCount.value = RatingStore.rankingsCount;
  } catch (err: unknown) {
    console.error(err);
  }
}

onMounted(() => {
  loadRankings();
});

function handleParameterChange() {
  if (!ratingType.value) return;
  loadRankings();
}

watch(pageEnd, loadRankings);
</script>
