<template>
  <LoadingPanel :status="ServerStore.status">
    <div class="d-flex justify-content-center">
      <div class="col-md-9 col-sm-12">
        <div class="row  mb-3">
          <div class="col-md-6 col-sm-12">
            <input type="text" class="form-control" v-model="filterText" placeholder="Filter..." />
          </div>
          <div class="col-md-6 col-sm-12 d-flex align-items-center justify-content-end">
            <div class="form-check d-flex align-items-center">
              <input class="form-check-input m-2" type="checkbox" value="" id="hideEmpty" v-model="hideEmpty" />
              <label class="form-check-label" for="hideEmpty">
                Hide Empty Hubs
              </label>
            </div>
          </div>
        </div>
        <div class="list-group">
          <div class="list-group-item list-group-item-action active">Hubs</div>
          <Hub :hub="hub" v-for="hub in filteredHubs" />
          <div v-if="!ServerStore.hubs.length" class="hub list-group-item list-group-item-action">
            <h5>No hubs online</h5>
          </div>
        </div>
        <div v-if="filteredHubs.length !== ServerStore.hubs.length" class="mt-2 d-flex justify-content-end">
          Showing {{ filteredHubs.length }} of {{ ServerStore.hubs.length }} hubs
        </div>
      </div>
    </div>
  </LoadingPanel>
</template>

<style lang="scss" scoped>
#searchContainer {
  margin-bottom: 1rem;
}
</style>

<script lang="ts" setup>
import { onMounted, computed, shallowRef } from 'vue';
import Hub from '@/components/Hub.vue';
import { ServerStore } from '@/stores/server-store';
import LoadingPanel from '@/components/LoadingPanel.vue';
import { AccountStore } from '@/stores/account-store';
import { orderBy } from 'lodash';
import { SessionStore } from '@/stores/session-store';

const filterText = shallowRef('');
const hideEmpty = shallowRef(true);
const sortedHubs = computed(() => orderBy(ServerStore.hubs, 'totalPlayers', 'desc'));
const emptyHubs = computed(() => sortedHubs.value.filter((h) => !(h.totalPlayers === 0 && hideEmpty.value)));
const filteredHubs = computed(() => emptyHubs.value.filter((h) => h.attributes.UT_SERVERNAME_s.toLocaleLowerCase().includes(filterText.value.toLocaleLowerCase())));

onMounted(() => {
  ServerStore.fetchGameServers();
  if (SessionStore.isAuthenticated) {
    AccountStore.fetchAllAccounts();
  }
});
</script>
