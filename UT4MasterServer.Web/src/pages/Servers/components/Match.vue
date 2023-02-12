<template>
  <div
    class="list-group-item list-group-item-action"
    @click.stop="toggleShowPlayers"
  >
    <div class="match">
      <h5>{{ match.name }}</h5>
      <div class="d-flex justify-content-between">
        <div>{{ match.map }}</div>
        <div>{{ match.playersOnline }} / {{ match.maxPlayers }} Players</div>
      </div>
      <div class="d-flex justify-content-between">
        <div>{{ match.matchStateDisplay }}</div>
        <div>
          Elapsed Time:
          {{ toMinutesSeconds(match.duration) }}
        </div>
      </div>
      <div v-if="match.mutators.length">
        {{ match.mutators.join(', ') }}
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
import { PropType, shallowRef } from 'vue';
import { toMinutesSeconds } from '@/utils/utilities';
import PlayersInMatch from './PlayersInMatch.vue';
import { SessionStore } from '@/stores/session-store';
import { IMatch } from '../types/match';

defineProps({
  match: {
    type: Object as PropType<IMatch>,
    required: true
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
