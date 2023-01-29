<template>
    <div class="hub list-group-item list-group-item-action" :class="{ 'active-hub': showMatches }"
        @click="showMatches = !showMatches">
        <h5 :title="hub?.attributes.UT_SERVERNAME_s">{{ hub.attributes.UT_SERVERNAME_s }}</h5>
        <div>
            IP: {{ hub.serverAddress }}
            <CopyButton :subject="hub.serverAddress" />
        </div>
        <div class="d-flex justify-content-between">
            <div>{{ hub.matches.length }} Matches</div>
            <div>{{ hub.totalPlayers }} Players</div>
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
import { PropType, shallowRef } from 'vue';
import MatchList from './MatchList.vue';
import CopyButton from '@/components/CopyButton.vue';

const props = defineProps({
    hub: {
        type: Object as PropType<IGameHub>,
        required: true
    }
});

const showMatches = shallowRef(false);

</script>
