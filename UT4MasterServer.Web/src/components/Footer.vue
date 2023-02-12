<template>
  <div class="footer container text-center">
    <div class="disclaimer">
      This website is not affiliated with Epic Games or the Unreal Tournament
      brand in any way.
    </div>
    <div class="links">
      <a
        class="btn btn-sm btn-icon"
        href="https://discord.gg/2DaCWkK"
        target="_blank"
      >
        <FontAwesomeIcon icon="fa-brands fa-discord" />
      </a>
      <a
        href="https://github.com/timiimit/UT4MasterServer"
        target="_blank"
        class="btn btn-sm btn-icon"
      >
        <FontAwesomeIcon icon="fa-brands fa-github" />
      </a>
    </div>
  </div>
  <nav class="navbar navbar-expand-lg navbar-dark bg-dark">
    <div class="container">
      <div class="online d-flex text-white">
        <!-- <div>Hubs Online: {{ hubs.length }}</div> -->
        <div>Matches In Progress: {{ matchesInProgress }}</div>
        <div>Players Online: {{ playersOnline }}</div>
      </div>
    </div>
  </nav>
</template>

<style lang="scss" scoped>
.footer {
  margin-bottom: 5rem;
  .disclaimer {
    font-size: 0.75rem;
    margin-bottom: 0.25rem;
  }

  .links {
    text-align: center;
    a.btn {
      margin-right: 1rem;
    }
  }
}

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

  .online {
    width: 100%;
    margin-bottom: 0.25rem;
    justify-content: space-between;
  }
}
</style>

<script setup lang="ts">
import { computed, onMounted, onUnmounted, shallowRef } from 'vue';
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';
import { useServers } from '@/pages/Servers/hooks/use-servers.hook';
import { ServerStore } from '@/pages/Servers/stores/server.store';
import { MatchState } from '@/pages/Servers/enums/match-state';

const pollTime = 30000;
// Seems to be a bug with eslint ts parser version
// eslint-disable-next-line no-undef
const timer = shallowRef<NodeJS.Timer | undefined>(undefined);

const { hubs, servers } = useServers();

const playersOnline = computed(() => {
  const playersInHubs = hubs.value.reduce((sum, h) => sum + h.totalPlayers, 0);
  const playersInServers = servers.value.reduce(
    (sum, s) => sum + s.playersOnline,
    0
  );
  // TODO: not sure how to determine players in quick play matches
  //const playersInQuickplay = servers.value.reduce((sum, q) => sum + q.playersOnline, 0);
  return playersInHubs + playersInServers;
});

const matchesInProgress = computed(() => {
  const hubMatches = hubs.value.reduce(
    (sum, h) =>
      sum +
      h.matches.filter((m) => m.matchState === MatchState.inProgress).length,
    0
  );

  const serverMatches = servers.value.filter(
    (s) => s.matchState === MatchState.inProgress
  ).length;

  // TODO: not sure how we can determine match in progress for quick play yet
  // const quickPlayMatches = quickPlayServers.value.filter(
  //   (q) => q.matchState === MatchState.inProgress
  // ).length;

  return hubMatches + serverMatches;
});

function poll() {
  ServerStore.fetchAllServers();
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
