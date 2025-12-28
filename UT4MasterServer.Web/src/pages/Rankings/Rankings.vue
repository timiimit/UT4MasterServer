<template>
	<LoadingPanel :status="status" auto-load @load="loadData">
		<div class="d-flex justify-content-center">
			<div class="col-md-6 col-sm-12 mb-4">
				<label for="ratingType" class="col-md-6 col-sm-12 col-form-label">
					Rating Type
				</label>
				<select
					v-model="ratingType"
					class="form-select"
					@change="handleRatingTypeChange"
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
							<th />
						</tr>
					</thead>
					<tbody>
						<Ranking
							v-if="selectedRanking && showAbove"
							selected-player
							:ranking="selectedRanking!"
							:page-size="pageSize"
							@set-page="handleSetPage"
						/>
						<Ranking
							v-for="ranking in rankings"
							:key="ranking.accountID"
							:ranking="ranking"
						/>
						<Ranking
							v-if="selectedRanking && showBelow"
							selected-player
							:ranking="selectedRanking"
							:page-size="pageSize"
							@set-page="handleSetPage"
						/>
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
					:key="pagingKey"
					:item-count="rankingsCount"
					:page-size="pageSize"
					:page="setPage"
					@update="handlePagingUpdate"
				/>
			</div>
		</div>
	</LoadingPanel>
</template>

<style>
img.flag {
	margin-right: 0.5rem;
}
</style>

<script lang="ts" setup>
import { shallowRef, watch, computed } from 'vue';
import { IRanking } from '@/types/ranking';
import { RatingType } from '@/enums/rating-type';
import { usePaging } from '@/hooks/use-paging.hook';
import LoadingPanel from '@/components/LoadingPanel.vue';
import Paging from '@/components/Paging.vue';
import { RankingsService } from '@/services/rankings.service';
import { AsyncStatus } from '@/types/async-status';
import Ranking from './components/Ranking.vue';
import { useRoute, useRouter } from 'vue-router';
import { SessionStore } from '@/stores/session-store';
import {
	getRouteParamNumberValue,
	getRouteParamStringValue
} from '@/utils/utilities';

const route = useRoute();
const router = useRouter();
const typeParam = getRouteParamStringValue(
	route.params,
	'type',
	RatingType.DMSkillRating
) as RatingType;
const pageParam = getRouteParamNumberValue(route.params, 'page', 1);
const { pageSize, pageStart, currentPage, handlePagingUpdate } = usePaging();
const setPage = shallowRef(pageParam - 1);
const pagingKey = shallowRef(0);

const ratingType = shallowRef(typeParam);
const status = shallowRef(AsyncStatus.OK);
const rankings = shallowRef<IRanking[]>([]);
const rankingsCount = shallowRef(0);
const selectedRanking = shallowRef<IRanking | undefined>(undefined);

const showSelectedRanking = computed(
	() =>
		selectedRanking.value &&
		!rankings.value.find(
			(r) => r.accountID === selectedRanking.value?.accountID
		)
);

const showAbove = computed(
	() =>
		showSelectedRanking.value &&
		selectedRanking.value &&
		selectedRanking.value.rank < rankings.value[0].rank
);

const showBelow = computed(
	() =>
		showSelectedRanking.value &&
		selectedRanking.value &&
		selectedRanking.value.rank > rankings.value[rankings.value.length - 1].rank
);

const ratingService = new RankingsService();

const ratingTypeOptions = [
	{ text: 'Deathmatch', value: RatingType.DMSkillRating },
	{ text: 'Duel', value: RatingType.SkillRating },
	{ text: 'Team Deathmatch', value: RatingType.TDMSkillRating },
	{ text: 'Capture the Flag', value: RatingType.CTFSkillRating },
	{ text: 'Showdown', value: RatingType.ShowdownSkillRating },
	{ text: 'Blitz', value: RatingType.FlagRunSkillRating }
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

async function loadSelectedRanking() {
	if (!ratingType.value || !SessionStore.session?.account_id) {
		return;
	}

	try {
		selectedRanking.value = await ratingService.getSelectedRanking(
			ratingType.value,
			SessionStore.session?.account_id
		);
	} catch (err: unknown) {
		//Do nothing
	}
}

async function loadData() {
	try {
		status.value = AsyncStatus.BUSY;
		await Promise.all([loadRankings(), loadSelectedRanking()]);
		status.value = AsyncStatus.OK;
	} catch (err: unknown) {
		status.value = AsyncStatus.ERROR;
		console.error(err);
	}
}

function handleRatingTypeChange() {
	if (!ratingType.value) {
		return;
	}
	handleSetPage(0);
}

function handleSetPage(page: number) {
	pagingKey.value++;
	setPage.value = page;
	router.push({
		name: 'Rankings',
		params: { type: ratingType.value, page: page + 1 }
	});
}

watch(currentPage, () => {
	router.push({
		name: 'Rankings',
		params: { type: ratingType.value, page: currentPage.value + 1 }
	});
});
</script>
