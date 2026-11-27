<template>
	<div
		class="list-group-item list-group-item-action"
		@click.stop="toggleShowPlayers"
	>
		<div class="server">
			<h5>{{ server.name }}</h5>
			<div class="d-flex justify-content-between">
				<div>{{ server.map }}</div>
				<div>{{ server.playersOnline }} / {{ server.maxPlayers }} Players</div>
			</div>
			<div class="d-flex justify-content-between">
				<div>{{ server.matchState }}</div>
				<div>
					Elapsed Time:
					{{ toMinutesSeconds(server.elapsedTime) }}
				</div>
			</div>
		</div>
		<PlayersInMatch v-if="playersVisible" :player-ids="server?.publicPlayers" />
	</div>
</template>

<style lang="scss" scoped>
h5,
div {
	text-transform: none;
	text-overflow: ellipsis;
	overflow: hidden;
	white-space: nowrap;
	max-width: 80vw;
}
</style>

<script setup lang="ts">
import { PropType, shallowRef } from 'vue';
import { toMinutesSeconds } from '@/utils/utilities';
import PlayersInMatch from './PlayersInMatch.vue';
import { SessionStore } from '@/stores/session-store';
import { IServer } from '../types/server';

defineProps({
	server: {
		type: Object as PropType<IServer>,
		required: true
	}
});

const playersVisible = shallowRef(false);

function toggleShowPlayers() {
	if (!SessionStore.isAuthenticated) {
		playersVisible.value = false;
		return;
	}
	playersVisible.value = !playersVisible.value;
}
</script>
