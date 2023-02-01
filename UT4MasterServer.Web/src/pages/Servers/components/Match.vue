<template>
  <div
    class="list-group-item list-group-item-action"
    @click.stop="toggleShowPlayers"
  >
    <div class="match">
      <h5>{{ match?.attributes.UT_SERVERNAME_s }}</h5>
      <div class="d-flex justify-content-between">
        <div>{{ match?.attributes.MAPNAME_s }}</div>
        <div>
          {{ match?.attributes.UT_PLAYERONLINE_i }} /
          {{ match?.attributes.UT_MAXPLAYERS_i }} Players
        </div>
      </div>
      <div class="d-flex justify-content-between">
        <div>{{ match?.attributes.UT_MATCHSTATE_s }}</div>
        <div>
          Elapsed Time:
          {{ toMinutesSeconds(match?.attributes.UT_MATCHDURATION_i) }}
        </div>
      </div>
    </div>
    <PlayersInMatch v-if="playersVisible" :player-ids="match?.publicPlayers" />
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
import { IGameServer } from '@/types/game-server';
import { PropType, shallowRef } from 'vue';
import { toMinutesSeconds } from '@/utils/utilities';
import PlayersInMatch from './PlayersInMatch.vue';
import { SessionStore } from '@/stores/session-store';

defineProps({
  match: {
    type: Object as PropType<IGameServer>,
    default: undefined
  }
});

const playersVisible = shallowRef(false);

function toggleShowPlayers() {
  if (!SessionStore.isAuthenticated) {
    playersVisible.value = false;
    return;
  }
  playersVisible.value = !playersVisible.value;
}
</script>
