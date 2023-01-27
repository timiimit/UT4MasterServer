<template>
    <a :href="hub ? '#' : undefined" class="hub list-group-item list-group-item-action"
        :title="hub?.attributes.UT_SERVERNAME_s" :class="{ 'active-hub': selectedHub?.id === hub?.id }">
        <template v-if="hub">
            <h5>{{ hub.attributes.UT_SERVERNAME_s }}</h5>
            <div>IP: <span class="hub-ip">{{ hub.serverAddress }}</span></div>
            <div class="flex-space-btw">
                <div>{{ hub.matches.length }} Matches</div>
                <div>{{ hub.totalPlayers }} Players</div>
            </div>
        </template>
        <h5 v-else>No hubs online</h5>
    </a>
</template>

<style lang="scss" scoped>
.hub {
    &.active-hub {
        z-index: 1;
        color: var(--bs-list-group-action-hover-color);
        text-decoration: none;
        background-color: var(--bs-list-group-action-hover-bg);
    }

    h5,
    div {
        text-transform: none;
        text-overflow: ellipsis;
        overflow: hidden;
        white-space: nowrap;
    }

    span.hub-ip {
        user-select: all;
    }
}
</style>

<script setup lang="ts">
import { IGameHub } from '@/types/game-server';
import { PropType } from 'vue';

defineProps({
    hub: {
        type: Object as PropType<IGameHub>,
        default: undefined
    },
    selectedHub: {
        type: Object as PropType<IGameHub>,
        default: undefined
    }
});
</script>
