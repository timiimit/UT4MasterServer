<template>
	<div
		class="hub list-group-item list-group-item-action"
		:class="{ 'active-hub': showMatches }"
		@click="showMatches = !showMatches"
	>
		<div class="d-flex align-items-center justify-content-between">
			<div class="server-name" :title="hub.serverName">
				{{ hub.serverName }}
			</div>
			<div class="text-info">
				<FontAwesomeIcon
					v-if="trustedHub"
					title="Trusted Hub"
					icon="fa-solid fa-certificate"
				/>
				<FontAwesomeIcon v-if="ut4uu" title="UT4UU" icon="fa-solid fa-u" />
			</div>
		</div>
		<div class="d-flex justify-content-between">
			<div>{{ hub.matches.length }} Matches</div>
			<div>{{ hub.totalPlayers }} Players</div>
		</div>
		<div v-if="hub.forcedMutators?.length" class="mutators">
			<strong>Mutators:</strong> {{ hub.forcedMutators?.join(', ') }}
		</div>
		<MatchList v-if="showMatches" :hub="hub" />
	</div>
</template>

<style lang="scss" scoped>
.hub {
	cursor: pointer;

	&.active-hub {
		z-index: 1;
		color: var(--bs-list-group-action-hover-color);
		text-decoration: none;
		background-color: var(--bs-list-group-action-hover-bg);
	}

	.server-name {
		font-size: 1.1rem;
		text-overflow: ellipsis;
		overflow: hidden;
		white-space: nowrap;
		max-width: 80vw;
	}

	.mutators {
		font-size: 0.7rem;
	}

	.text-info svg:not(:first-child) {
		margin-left: 0.5rem;
	}
}
</style>

<script setup lang="ts">
import { GameServerTrust } from '@/enums/game-server-trust';
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';
import { PropType, shallowRef, computed } from 'vue';
import { IHub } from '../types/hub';
import MatchList from './MatchList.vue';

const props = defineProps({
	hub: {
		type: Object as PropType<IHub>,
		required: true
	}
});

const showMatches = shallowRef(false);

const trustedHub = computed(
	() => props.hub.serverTrustLevel !== GameServerTrust.Untrusted
);

const ut4uu = computed(() => props.hub.uuInstalled);
</script>
