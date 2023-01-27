<template>
    <div class="list-group">
        <a class="list-group-item list-group-item-action active">
            Matches: {{ hub.attributes.UT_SERVERNAME_s }}
            <!-- Commenting this out for now because MOTD contains some weird proprietary markup -->
            <!-- <div class="server-message">{{ hub.attributes.UT_SERVERMOTD_s }}</div> -->
        </a>
        <Match :match="match" v-for="match in hub.matches" />
        <a v-if="!hub.matches.length" class="list-group-item list-group-item-action">
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
import { IGameHub } from '@/types/game-server';
import { PropType } from 'vue';
import Match from './Match.vue';

defineProps({
    hub: {
        type: Object as PropType<IGameHub>,
        required: true
    }
});
</script>
