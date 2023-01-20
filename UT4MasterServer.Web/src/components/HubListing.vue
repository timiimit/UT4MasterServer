<template>
    <div class="list-group">
        <a class="list-group-item list-group-item-action active">
            Matches: {{ hub.attributes.UT_SERVERNAME_s }}
            <!-- Commenting this out for now because MOTD contains some weird proprietary markup -->
            <!-- <div class="server-message">{{ hub.attributes.UT_SERVERMOTD_s }}</div> -->
        </a>
        <a href="#" v-for="match in hub.matches" class="list-group-item list-group-item-action">
            <div class="match">
                <h5>{{ match.attributes.UT_SERVERNAME_s }}</h5>
                <div class="flex-space-btw">
                    <div>{{ match.attributes.MAPNAME_s }}</div>
                    <div>{{ match.attributes.UT_PLAYERONLINE_i }} / {{ match.attributes.UT_MAXPLAYERS_i }} Players
                    </div>
                </div>
                <div class="flex-space-btw">
                    <div>{{ match.attributes.UT_MATCHSTATE_s }}</div>
                    <div>Elapsed Time: {{ match.attributes.UT_MATCHDURATION_i }}</div>
                </div>
            </div>
        </a>
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
import { IGameHub } from '../types/game-server';
import { PropType } from 'vue';

defineProps({
    hub: {
        type: Object as PropType<IGameHub>,
        required: true
    }
});
</script>
