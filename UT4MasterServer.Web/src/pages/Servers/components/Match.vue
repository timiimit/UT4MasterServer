<template>
  <div
    class="list-group-item list-group-item-action"
    @click.stop="toggleShowPlayers"
  >
    <div class="match">
      <h5>{{ match.name }}</h5>
      <div class="d-flex justify-content-between">
        <div>{{ match.gameModeDisplay }} in {{ match.map }}</div>
        <div>{{ match.playersOnline }} / {{ match.maxPlayers }} Players</div>
      </div>
      <div class="d-flex justify-content-between">
        <div>
          Duration:
          {{ toMinutesSeconds(match.duration) }}
        </div>
        <div>
          Elapsed Time:
          {{ toMinutesSeconds(match.elapsedTime) }}
        </div>
      </div>
      <div class="d-flex justify-content-between">
        <div>{{ match.matchStateDisplay }}</div>
        <div v-if="match.teamScores?.split(',').length === 2" class="scores">
          <div>
            <label>Red Team Score:</label> {{ match.teamScores.split(',')[0] }}
          </div>
          <div>
            <label>Blue Team Score:</label> {{ match.teamScores.split(',')[1] }}
          </div>
        </div>
      </div>

      <div v-if="match.mutators.length" class="mutators">
        <strong>Mutators: </strong>{{ mutators.join(', ') }}
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

.scores {
  text-align: right;
  font-size: 0.8rem;
}

.mutators {
  font-size: 0.7rem;
  white-space: normal;
}
</style>

<script setup lang="ts">
import { PropType, shallowRef, computed } from 'vue';
import { toMinutesSeconds } from '@/utils/utilities';
import PlayersInMatch from './PlayersInMatch.vue';
import { SessionStore } from '@/stores/session-store';
import { IMatch } from '../types/match';
import { IHub } from '../types/hub';

const props = defineProps({
  match: {
    type: Object as PropType<IMatch>,
    required: true
  },
  hub: {
    type: Object as PropType<IHub>,
    required: true
  }
});

const playersVisible = shallowRef(false);

const mutators = computed(() => {
  // Union of match mutators and forced mutators
  const matchMutators = Array.from(
    new Set([...props.match.mutators, ...props.match.forcedMutators])
  );
  // excluding the hub's forced mutators because they are already shown at the hub level
  return matchMutators.filter((m) => !props.hub.forcedMutators.includes(m));
});

function toggleShowPlayers() {
  if (!SessionStore.isAuthenticated) {
    playersVisible.value = false;
    return;
  }
  playersVisible.value = !playersVisible.value;
}
</script>
