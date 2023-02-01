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
    <div v-show="playersVisible" class="alert alert-primary">
      <h6>Players in match</h6>
      <div class="players">
        <div v-for="playerId in match?.publicPlayers" :key="playerId">
          {{ getPlayerName(playerId) }}
        </div>
      </div>
    </div>
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
import { IGameServer } from '@/types/game-server';
import { PropType, shallowRef } from 'vue';
import { AccountStore } from '@/stores/account-store';
import { toMinutesSeconds } from '@/utils/utilities';

defineProps({
  match: {
    type: Object as PropType<IGameServer>,
    default: undefined,
  },
});

const playersVisible = shallowRef(false);

function toggleShowPlayers() {
  if (!AccountStore.accounts?.length) {
    playersVisible.value = false;
    return;
  }
  playersVisible.value = !playersVisible.value;
}

function getPlayerName(id: string) {
  const player = AccountStore.accounts?.find((a) => a.ID === id);
  return player?.Username;
}
</script>
