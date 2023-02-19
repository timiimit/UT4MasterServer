<template>
  <div class="list-group">
    <div class="list-group-item list-group-item-action active">Hubs</div>
    <Hub v-for="hub in filteredHubs" :key="hub.id" :hub="hub" />
    <div v-if="!hubs.length" class="hub list-group-item list-group-item-action">
      <p class="mt-3">No hubs online</p>
    </div>
    <div
      v-if="hubs.length && !filteredHubs.length"
      class="hub list-group-item list-group-item-action"
    >
      <p class="mt-3">No hubs matching filters</p>
    </div>
  </div>
  <div
    v-if="filteredHubs.length !== hubs.length"
    class="mt-2 d-flex justify-content-end"
  >
    Showing {{ filteredHubs.length }} of {{ hubs.length }} hubs
  </div>
</template>

<script lang="ts" setup>
import { orderBy } from 'lodash';
import { computed } from 'vue';
import { useServers } from '../hooks/use-servers.hook';
import Hub from './Hub.vue';

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

const { hubs } = useServers();

const sortedHubs = computed(() =>
  orderBy(hubs.value, ['serverTrustLevel', 'totalPlayers'], ['asc', 'desc'])
);

const filteredHubs = computed(() =>
  sortedHubs.value
    .filter((h) => !(h.totalPlayers === 0 && props.hideEmpty))
    .filter((h) =>
      h.serverName
        .toLocaleLowerCase()
        .includes(props.filterText.toLocaleLowerCase())
    )
);
</script>
