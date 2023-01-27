<template>
    <a href="#" class="list-group-item list-group-item-action" @click="toggleShowPlayers">
        <div class="match">
            <h5>{{ match?.attributes.UT_SERVERNAME_s }}</h5>
            <div class="flex-space-btw">
                <div>{{ match?.attributes.MAPNAME_s }}</div>
                <div>{{ match?.attributes.UT_PLAYERONLINE_i }} / {{ match?.attributes.UT_MAXPLAYERS_i }} Players
                </div>
            </div>
            <div class="flex-space-btw">
                <div>{{ match?.attributes.UT_MATCHSTATE_s }}</div>
                <div>Elapsed Time: {{ match?.attributes.UT_MATCHDURATION_i }}</div>
            </div>
        </div>
        <div class="alert alert-primary" v-show="playersVisible">
            <h6>Players in match</h6>
            <div class="players">
                <div v-for="playerId in match?.publicPlayers">
                    {{ getPlayerName(playerId) }}
                </div>
            </div>
        </div>
    </a>
</template>

<style lang="scss" scoped>
h5,
div {
    text-transform: none;
    text-overflow: ellipsis;
    overflow: hidden;
    white-space: nowrap;
}

.alert .players {
    display: flex;
    justify-content: space-between;
    flex-wrap: wrap;
    div {
        margin: 0 0.5rem;
    }
}
</style>

<script setup lang="ts">
import { IGameServer } from '@/types/game-server';
import { PropType, shallowRef } from 'vue';
import { AccountStore } from '@/stores/account-store';

defineProps({
    match: {
        type: Object as PropType<IGameServer>,
        default: undefined
    }
});

const playersVisible = shallowRef(false);

function toggleShowPlayers() {
    playersVisible.value = !playersVisible.value;
}

function getPlayerName(id: string) {
    const player = AccountStore.accounts?.find((a) => a.id === id);
    return player?.displayName;
}
</script>

