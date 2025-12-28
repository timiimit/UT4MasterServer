<template>
	<div class="list-group pt-2">
		<Match
			v-for="match in hub.matches"
			:key="match.id"
			:match="match"
			:hub="hub"
		/>
		<a
			v-if="!hub.matches.length"
			class="list-group-item list-group-item-action"
		>
			<div class="match">
				<h5>No matches in progress</h5>
			</div>
		</a>
	</div>
</template>

<style lang="scss" scoped>
h5,
div {
	text-transform: none;
	text-overflow: ellipsis;
	overflow: hidden;
	white-space: nowrap;
}
</style>

<script setup lang="ts">
import { PropType } from 'vue';
import { IHub } from '../types/hub';
import Match from './Match.vue';

defineProps({
	hub: {
		type: Object as PropType<IHub>,
		required: true
	}
});
</script>
