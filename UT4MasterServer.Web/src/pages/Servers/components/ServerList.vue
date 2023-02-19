<template>
  <div class="list-group">
    <div class="list-group-item list-group-item-action active">Servers</div>
    <Server
      v-for="server in filteredServers"
      :key="server.id"
      :server="server"
    />
    <div
      v-if="!servers.length"
      class="hub list-group-item list-group-item-action"
    >
      <p class="mt-3">No servers online</p>
    </div>
    <div
      v-if="servers.length && !filteredServers.length"
      class="hub list-group-item list-group-item-action"
    >
      <p class="mt-3">No servers matching filters</p>
    </div>
  </div>
  <div
    v-if="filteredServers.length !== servers.length"
    class="mt-2 d-flex justify-content-end"
  >
    Showing {{ filteredServers.length }} of {{ servers.length }} servers
  </div>
</template>

<script lang="ts" setup>
import { orderBy } from 'lodash';
import { computed } from 'vue';
import { useServers } from '../hooks/use-servers.hook';
import Server from './Server.vue';

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

const { servers } = useServers();

const sortedServers = computed(() =>
  orderBy(servers.value, ['serverTrustLevel', 'playersOnline'], ['asc', 'desc'])
);

const filteredServers = computed(() =>
  sortedServers.value
    .filter((s) => !(s.playersOnline === 0 && props.hideEmpty))
    .filter((s) =>
      s.name.toLocaleLowerCase().includes(props.filterText.toLocaleLowerCase())
    )
);
</script>
