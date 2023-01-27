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
        <MatchList :hub="selectedHub" />
      </div>
    </div>
  </LoadingPanel>
</template>

<script lang="ts" setup>
import { IGameHub } from '@/types/game-server';
import { onMounted, shallowRef, computed } from 'vue';
import Hub from '@/components/Hub.vue';
import MatchList from '@/components/MatchList.vue';
import { ServerStore } from '@/stores/server-store';
import LoadingPanel from '@/components/LoadingPanel.vue';
import { AccountStore } from '@/stores/account-store';

const selectedHubId = shallowRef<string | undefined>(undefined);
const selectedHub = computed(() => ServerStore.hubs.find((h) => h.attributes.UT_SERVERINSTANCEGUID_s === selectedHubId.value));

function viewHub(hub: IGameHub) {
  selectedHubId.value = hub.attributes.UT_SERVERINSTANCEGUID_s;
}

async function loadAccounts() {
  try {
    if (AccountStore.accounts?.length) {
      return;
    }
    await AccountStore.fetchAllAccounts();
  }
  catch (err: unknown) {
    console.error(err);
  }
}

onMounted(() => {
  ServerStore.fetchGameServers();
  loadAccounts();
});
</script>

