<template>
	<div class="list-group-item list-group-item-action">
		<div class="server">
			<h5>Quick Play {{ server.gameModeDisplay }}</h5>
			<div class="d-flex justify-content-between">
				<div><label>Region:</label>{{ server.region }}</div>
				<div v-if="server.targetElo">
					<label>Target Elo:</label>
					{{ server.targetElo }}
				</div>
			</div>
		</div>
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
import { PropType } from 'vue';
import { IQuickPlayServer } from '../types/quick-play-server';

defineProps({
	server: {
		type: Object as PropType<IQuickPlayServer>,
		required: true
	}
});
</script>
