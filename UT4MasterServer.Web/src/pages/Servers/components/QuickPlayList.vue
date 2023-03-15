<template>
  <div class="list-group">
    <div class="list-group-item list-group-item-action active">
      Quick Play Servers
    </div>
    <QuickPlayServer
      v-for="server in filteredServers"
      :key="server.id"
      :server="server"
    />
    <div
      v-if="!quickPlayServers.length"
      class="hub list-group-item list-group-item-action"
    >
      <p class="mt-3">No Quick Play Servers online</p>
    </div>
    <div
      v-if="quickPlayServers.length && !filteredServers.length"
      class="hub list-group-item list-group-item-action"
    >
      <p class="mt-3">No Quick Play Servers matching filters</p>
    </div>
  </div>
  <div
    v-if="filteredServers.length !== quickPlayServers.length"
    class="mt-2 d-flex justify-content-end"
  >
    Showing {{ filteredServers.length }} of
    {{ quickPlayServers.length }} servers
  </div>
</template>

<script lang="ts" setup>
import { orderBy } from 'lodash';
import { computed } from 'vue';
import { GameMode } from '../enums/game-mode';
import { useQuickPlay } from '../hooks/use-quick-play.hook';
import QuickPlayServer from './QuickPlayServer.vue';

const props = defineProps({
  filterText: {
    type: String,
    default: ''
  },
  hideEmpty: {
    type: Boolean,
    default: true
  }
});

const { quickPlayServers } = useQuickPlay();

const sortedServers = computed(() =>
  orderBy(quickPlayServers.value, 'playersOnline', 'desc')
);

const filteredServers = computed(() =>
  sortedServers.value
    .filter((s) => !(s.gameMode === GameMode.empty && props.hideEmpty))
    .filter((s) =>
      s.gameModeDisplay
        .toLocaleLowerCase()
        .includes(props.filterText.toLocaleLowerCase())
    )
);
</script>
