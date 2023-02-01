<template>
  <div
    class="hub list-group-item list-group-item-action"
    :class="{ 'active-hub': showMatches }"
    @click="showMatches = !showMatches"
  >
    <div class="d-flex align-items-center justify-content-between">
      <div class="server-name" :title="hub.attributes.UT_SERVERNAME_s">
        {{ hub.attributes.UT_SERVERNAME_s }}
      </div>
      <div class="text-info" title="Trusted Hub">
        <FontAwesomeIcon v-if="trustedHub" icon="fa-solid fa-certificate" />
      </div>
    </div>
    <div class="d-flex justify-content-between">
      <div>{{ hub.matches.length }} Matches</div>
      <div>{{ hub.totalPlayers }} Players</div>
    </div>
    <MatchList v-if="showMatches" :hub="hub" />
  </div>
</template>

<style lang="scss" scoped>
.hub {
  cursor: pointer;

  &.active-hub {
    z-index: 1;
    color: var(--bs-list-group-action-hover-color);
    text-decoration: none;
    background-color: var(--bs-list-group-action-hover-bg);
  }

  .server-name {
    font-size: 1.1rem;
    text-overflow: ellipsis;
    overflow: hidden;
    white-space: nowrap;
    max-width: 80vw;
  }
}
</style>

<script setup lang="ts">
import { IGameHub } from '@/types/game-server';
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';
import { PropType, shallowRef, computed } from 'vue';
import MatchList from './MatchList.vue';

const props = defineProps({
  hub: {
    type: Object as PropType<IGameHub>,
    required: true,
  },
});

const showMatches = shallowRef(false);

const trustedHub = computed(
  () =>
    props.hub.attributes.UT_SERVERTRUSTLEVEL_i &&
    props.hub.attributes.UT_SERVERTRUSTLEVEL_i < 2
);
</script>
