<template>
    <!-- TODO: Not really sure about any of these properties, hard to tell from test data -->
    <div class="list-group">
        <a class="list-group-item list-group-item-action active">
            Matches: {{ hub.serverName }}
            <div class="server-message">{{ hub.attributes.UT_SERVERMOTD_s }}</div>
        </a>
        <a href="#" v-for="match in hub.matches" class="list-group-item list-group-item-action">
            <div class="match">
                <h5>{{ match.serverName }}</h5>
                <div class="flex-space-btw">
                    <div>{{ match.attributes.MAPNAME_s }}</div>
                    <div>{{ match.attributes.UT_PLAYERONLINE_i }} / {{ match.attributes.UT_MAXPLAYERS_i }} Players
                    </div>
                </div>
                <div class="flex-space-btw">
                    <!-- <div>{{ match.attributes.GAMEMODE_s }}</div> -->
                    <div>{{ match.attributes.UT_MATCHSTATE_s }}</div>
                    <div>Elapsed Time: {{ match.attributes.UT_MATCHDURATION_i }}</div>
                </div>
            </div>
        </a>
    </div>
</template>

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
