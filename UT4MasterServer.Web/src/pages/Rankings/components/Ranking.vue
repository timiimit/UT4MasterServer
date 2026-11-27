<template>
	<tr
		:class="`${
			AccountStore.account?.id === ranking.accountID || selectedPlayer
				? 'table-info'
				: ''
		}`"
	>
		<td>{{ ranking.rank }}</td>
		<td>
			<img class="flag" :src="`/assets/flags/${ranking.countryFlag}.png`" />
			<router-link
				:to="{
					name: 'Stats',
					params: {
						window: StatisticWindow.AllTime,
						accountId: ranking.accountID
					}
				}"
			>
				{{ ranking.player }}</router-link
			>
		</td>
		<td>{{ ranking.rating }}</td>
		<td>{{ ranking.gamesPlayed }}</td>
		<td>
			<button
				v-if="selectedPlayer"
				class="btn btn-icon"
				title="Jump to page"
				@click="jumpToSelectedPlayer"
			>
				<FontAwesomeIcon icon="fa-solid fa-share" />
			</button>
		</td>
	</tr>
</template>

<script lang="ts" setup>
import { StatisticWindow } from '@/enums/statistic-window';
import { AccountStore } from '@/stores/account-store';
import { IRanking } from '@/types/ranking';
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';
import { PropType } from 'vue';

const props = defineProps({
	ranking: {
		type: Object as PropType<IRanking>,
		required: true
	},
	selectedPlayer: {
		type: Boolean,
		default: false
	},
	pageSize: {
		type: Number,
		default: 10
	}
});

const emit = defineEmits(['set-page']);

function jumpToSelectedPlayer() {
	const page = Math.ceil(props.ranking.rank / props.pageSize) - 1;
	emit('set-page', page);
}
</script>
