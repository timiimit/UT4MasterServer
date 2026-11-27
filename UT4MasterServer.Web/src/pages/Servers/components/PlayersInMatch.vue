<template>
	<div class="alert alert-primary">
		<h6>Players in match</h6>
		<div class="players">
			<div v-for="player in players" :key="player.id">
				{{ player.username }}
			</div>
		</div>
	</div>
</template>

<style lang="scss" scoped>
.alert {
	margin-top: 0.5rem;

	.players {
		display: flex;
		justify-content: space-between;
		flex-wrap: wrap;

		div {
			margin: 0 0.5rem;
		}
	}
}
</style>

<script setup lang="ts">
import { PropType, shallowRef, watch, onMounted } from 'vue';
import { AsyncStatus } from '@/types/async-status';
import { IAccount } from '@/types/account';
import AccountService from '@/services/account.service';

const props = defineProps({
	playerIds: {
		type: Array as PropType<string[]>,
		default: undefined
	}
});
const accountService = new AccountService();
const status = shallowRef(AsyncStatus.OK);
const players = shallowRef<IAccount[]>([]);

async function getPlayers() {
	if (!props.playerIds?.length) {
		return;
	}
	try {
		status.value = AsyncStatus.BUSY;
		players.value = await accountService.getAccountsByIds(props.playerIds);
		status.value = AsyncStatus.OK;
	} catch (err: unknown) {
		status.value = AsyncStatus.ERROR;
	}
}

watch(() => props.playerIds, getPlayers);
onMounted(getPlayers);
</script>
