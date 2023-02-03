<template>
  <LoadingPanel :status="ServerStore.status">
    <div class="d-flex justify-content-center">
      <div class="col-md-9 col-sm-12">
        <div class="row mb-3">
          <div class="col-md-6 col-sm-12">
            <input
              v-model="filterText"
              type="text"
              class="form-control"
              placeholder="Filter..."
            />
          </div>
          <div
            class="col-md-6 col-sm-12 d-flex align-items-center justify-content-end"
          >
            <div class="form-check d-flex align-items-center">
              <input
                id="hideEmpty"
                v-model="hideEmpty"
                class="form-check-input m-2"
                type="checkbox"
                value=""
              />
              <label class="form-check-label" for="hideEmpty">
                Hide Empty Hubs
              </label>
            </div>
          </div>
        </div>
        <div class="list-group">
          <div class="list-group-item list-group-item-action active">Hubs</div>
          <Hub v-for="hub in filteredHubs" :key="hub.id" :hub="hub" />
          <div
            v-if="!ServerStore.hubs.length"
            class="hub list-group-item list-group-item-action"
          >
            <h5>No hubs online</h5>
          </div>
        </div>
        <div
          v-if="filteredHubs.length !== ServerStore.hubs.length"
          class="mt-2 d-flex justify-content-end"
        >
          Showing {{ filteredHubs.length }} of
          {{ ServerStore.hubs.length }} hubs
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
import { computed, shallowRef } from 'vue';
import Hub from '@/pages/Servers/components/Hub.vue';
import { ServerStore } from '@/stores/server-store';
import LoadingPanel from '@/components/LoadingPanel.vue';
import { orderBy } from 'lodash';

const filterText = shallowRef('');
const hideEmpty = shallowRef(true);
const sortedHubs = computed(() =>
  orderBy(
    ServerStore.hubs,
    ['attributes.UT_SERVERTRUSTLEVEL_i', 'totalPlayers'],
    ['asc', 'desc']
  )
);
const emptyHubs = computed(() =>
  sortedHubs.value.filter((h) => !(h.totalPlayers === 0 && hideEmpty.value))
);
const filteredHubs = computed(() =>
  emptyHubs.value.filter((h) =>
    h.attributes.UT_SERVERNAME_s.toLocaleLowerCase().includes(
      filterText.value.toLocaleLowerCase()
    )
  )
);
</script>
