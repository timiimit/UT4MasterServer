<template>
  <nav class="navbar navbar-expand-lg navbar-dark bg-dark">
    <div class="container text-white">
      <div>Hubs Online: {{ ServerStore.hubs.length }}</div>
      <div>Matches In Progress: {{ matchesInProgress }}</div>
      <div>Players Online: {{ playersOnline }}</div>
    </div>

    <button
      class="btn btn-secondary btn-sm btn-smaller"
      @click="ServerStore.fetchGameServers"
    >
      Refresh
    </button>
  </nav>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted, shallowRef } from 'vue';
import { ServerStore } from '@/stores/server-store';

const pollTime = 30000;
// Seems to be a bug with eslint ts parser version
// eslint-disable-next-line no-undef
const timer = shallowRef<NodeJS.Timer | undefined>(undefined);

const playersOnline = computed(() => {
  return ServerStore.hubs.reduce((sum, s) => sum + s.totalPlayers, 0);
});

const matchesInProgress = computed(() => {
  return ServerStore.hubs.reduce(
    (sum, h) =>
      sum +
      h.matches.filter((m) => m.attributes.UT_MATCHSTATE_s === 'InProgress')
        .length,
    0
  );
});

function poll() {
  ServerStore.fetchGameServers();
}

function stopPolling() {
  clearInterval(timer.value);
  timer.value = undefined;
}

function startPolling() {
  if (timer.value) {
    return;
  }
  timer.value = setInterval(poll, pollTime);
}

function handleVisibilityChange() {
  if (document.visibilityState === 'visible') {
    startPolling();
  } else {
    stopPolling();
  }
}

onMounted(() => {
  poll();
  startPolling();
  window.onfocus = startPolling;
  window.onblur = stopPolling;
  document.addEventListener('visibilitychange', handleVisibilityChange);
});

onUnmounted(() => {
  window.onfocus = null;
  window.onblur = null;
  document.removeEventListener('visibilitychange', handleVisibilityChange);
});
</script>

<style lang="scss" scoped>
.navbar {
  width: 100%;
  position: fixed;
  bottom: 0;
  padding: 10px;
  z-index: 10;

  .container {
    display: flex;
    justify-content: space-between;
  }
}
</style>
