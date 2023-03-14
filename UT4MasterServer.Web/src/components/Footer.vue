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
import { computed, onMounted } from 'vue';
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';
import { useServers } from '@/pages/Servers/hooks/use-servers.hook';
import { ServerStore } from '@/pages/Servers/stores/server.store';
import { MatchState } from '@/pages/Servers/enums/match-state';
import { useHubs } from '@/pages/Servers/hooks/use-hubs.hook';
import { useQuickPlay } from '@/pages/Servers/hooks/use-quick-play.hook';

const { hubs } = useHubs();
const { servers } = useServers();
const { quickPlayServers } = useQuickPlay();

const playersOnline = computed(() => {
  const playersInHubs = hubs.value.reduce((sum, h) => sum + h.totalPlayers, 0);
  const playersInServers = servers.value.reduce(
    (sum, s) => sum + (s.playersOnline ?? 0),
    0
  );
  const playersInQuickplay = quickPlayServers.value.reduce(
    (sum, q) => sum + (q.playersOnline ?? 0),
    0
  );

  return playersInHubs + playersInServers + playersInQuickplay;
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

  const quickPlayMatches = quickPlayServers.value.filter(
    (q) => q.matchState === MatchState.inProgress
  ).length;

  return hubMatches + serverMatches + quickPlayMatches;
});

onMounted(() => {
  ServerStore.fetchAllServers();
});
</script>
