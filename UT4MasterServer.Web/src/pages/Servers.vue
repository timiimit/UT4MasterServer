<template>
  <LoadingPanel :status="ServerStore.status">
    <div class="row">
      <div class="col-sm-6">
        <div class="list-group">
          <a class="list-group-item list-group-item-action active">Hubs</a>
          <Hub :hub="hub" v-for="hub in ServerStore.hubs" :selected-hub="selectedHub" @click="viewHub(hub)" />
          <Hub v-if="!ServerStore.hubs.length" />
        </div>
        <!-- TODO: Should(?) also have Servers listed here in addition to Hubs, but I'm not yet sure how to differentiate -->
      </div>
      <div v-if="selectedHub" class="col-sm-6">
        <HubListing :hub="selectedHub" />
      </div>
    </div>
  </LoadingPanel>
</template>

<script lang="ts" setup>
import { IGameHub } from '../types/game-server';
import { onMounted, shallowRef } from 'vue';
import Hub from '../components/Hub.vue';
import HubListing from '../components/HubListing.vue';
import { ServerStore } from '../stores/server-store';
import LoadingPanel from '../components/LoadingPanel.vue';

const selectedHub = shallowRef<IGameHub | undefined>(undefined);

function viewHub(hub: IGameHub) {
  selectedHub.value = hub;
}

onMounted(ServerStore.fetchGameServers);
</script>

