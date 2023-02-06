<template>
  <nav class="navbar navbar-expand-lg navbar-dark bg-dark">
    <div class="container">
      <div class="d-flex justify-content-between text-white">
        <div>Hubs Online: {{ ServerStore.hubs.length }}</div>
        <div>Matches In Progress: {{ matchesInProgress }}</div>
        <div>Players Online: {{ playersOnline }}</div>
      </div>
      <div class="text-center disclaimer">
        This website is not affiliated with Epic Games or the Unreal Tournament
        brand in any way.
      </div>
      <div class="links d-flex justify-content-around">
        <a
          class="btn btn-sm btn-icon text-white"
          href="https://discord.gg/2DaCWkK"
          target="_blank"
        >
          <FontAwesomeIcon icon="fa-brands fa-discord" />
        </a>
        <a
          href="https://github.com/timiimit/UT4MasterServer"
          target="_blank"
          class="btn btn-sm btn-icon text-white"
        >
          <FontAwesomeIcon icon="fa-brands fa-github" />
        </a>
      </div>
    </div>
  </nav>
</template>

<style lang="scss" scoped>
.navbar {
  width: 100%;
  position: fixed;
  bottom: 0;
  padding: 10px;
  z-index: 10;
}

.container {
  display: flex;
  flex-direction: column;

  > div:first-child {
    width: 100%;
    margin-bottom: 0.25rem;
  }

  .disclaimer {
    font-size: 0.5rem;
    color: white;
    margin-bottom: 0.25rem;
  }

  .links {
    a.btn {
      margin-right: 1rem;
    }
  }
}
</style>

<script setup lang="ts">
import { computed, onMounted, onUnmounted, shallowRef } from 'vue';
import { ServerStore } from '@/stores/server-store';
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';

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
