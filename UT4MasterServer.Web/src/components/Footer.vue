<template>
  <nav class="navbar navbar-expand-lg navbar-dark bg-dark">
    <div class="container text-white">
      <div>
        Hubs Online: {{ ServerStore.hubs.length }}
      </div>
      <div>
        Matches In Progress: {{ matchesInProgress }}
      </div>
      <div>
        Players Online: {{ playersOnline }}
      </div>
    </div>

    <button class="btn btn-secondary btn-sm btn-smaller" @click="ServerStore.fetchGameServers">Refresh</button>
  </nav>
</template>

<style lang="scss" scoped>
.navbar {
  width: 100%;
  position: fixed;
  bottom: 0;
  padding: 10px;

  .container {
    display: flex;
    justify-content: space-between;
  }
}
</style>
  
<script setup lang="ts">
import { computed, onMounted } from 'vue';
import { ServerStore } from '../stores/server-store';

const playersOnline = computed(() => {
  return ServerStore.allServers.reduce((sum, s) => sum + s.attributes.UT_PLAYERONLINE_i, 0);
});
const matchesInProgress = computed(() => {
  return ServerStore.hubs.reduce((sum, h) => sum + h.matches.length, 0);
});

// TODO: maybe poll this every 60 seconds or something
onMounted(ServerStore.fetchGameServers);
</script>
