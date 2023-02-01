<template>
  <CrudPage title="Trusted Servers">
    <template #add="p">
      <AddTrustedServer
        :clients="allClients"
        :servers="trustedServers"
        @cancel="p.cancel"
        @added="
          loadTrustedServers();
          p.cancel();
        "
      />
    </template>
    <template #filters>
      <div>
        <input
          v-model="filterText"
          type="text"
          class="form-control"
          placeholder="Filter by Client Name..."
        />
      </div>
    </template>
    <LoadingPanel :status="status" auto-load @load="loadTrustedServers">
      <table class="table">
        <thead>
          <tr>
            <th>Client Name</th>
            <th>Owner</th>
            <th>Trust Level</th>
            <th />
          </tr>
        </thead>
        <tbody>
          <template
            v-for="trustedServer in filteredTrustedServers.slice(
              pageStart,
              pageEnd
            )"
            :key="objectHash(trustedServer)"
          >
            <tr :class="{ 'table-light': trustedServer.editing }">
              <td>{{ trustedServer.client?.name }}</td>
              <td>{{ trustedServer.owner?.Username }}</td>
              <td>{{ GameServerTrust[trustedServer.trustLevel] }}</td>
              <td class="actions">
                <button
                  class="btn btn-icon"
                  @click="trustedServer.editing = !trustedServer.editing"
                >
                  <FontAwesomeIcon icon="fa-regular fa-pen-to-square" />
                </button>
                <button
                  class="btn btn-icon"
                  @click="handleDelete(trustedServer)"
                >
                  <FontAwesomeIcon icon="fa-solid fa-trash-can" />
                </button>
              </td>
            </tr>
            <tr v-if="trustedServer.editing" class="edit-row table-light">
              <td colspan="4">
                <EditTrustedServer
                  :server="trustedServer"
                  :servers="trustedServers"
                  :clients="allClients"
                  @cancel="trustedServer.editing = false"
                  @updated="handleUpdated(trustedServer)"
                />
              </td>
            </tr>
          </template>
          <tr v-if="trustedServers.length === 0">
            <td colspan="4">No trusted servers</td>
          </tr>
        </tbody>
      </table>
      <Paging
        :items="filteredTrustedServers"
        :page-size="pageSize"
        @update="handlePagingUpdate"
      />
    </LoadingPanel>
  </CrudPage>
</template>

<style lang="scss" scoped>
td.actions {
  width: 6rem;

  button:not(:last-child) {
    margin-right: 1rem;
  }
}
</style>

<script lang="ts" setup>
import { shallowRef, ref, computed } from 'vue';
import CrudPage from '@/components/CrudPage.vue';
import LoadingPanel from '@/components/LoadingPanel.vue';
import { AsyncStatus } from '@/types/async-status';
import { objectHash } from '@/utils/utilities';
import AdminService from '@/services/admin-service';
import { ITrustedGameServer } from './types/trusted-game-server';
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';
import AddTrustedServer from './components/AddTrustedServer.vue';
import Paging from '@/components/Paging.vue';
import { IClient } from '../Clients/types/client';
import EditTrustedServer from './components/EditTrustedServer.vue';
import { GameServerTrust } from '@/enums/game-server-trust';
import { usePaging } from '@/hooks/use-paging.hook';
import { IAccount } from '@/types/account';
import { AccountStore } from '@/stores/account-store';

interface IGridTrustedServer extends ITrustedGameServer {
  editing?: boolean;
  client: IClient;
  owner?: IAccount;
}

const adminService = new AdminService();
const trustedServers = ref<IGridTrustedServer[]>([]);
const status = shallowRef(AsyncStatus.OK);
const filterText = shallowRef('');
const filteredTrustedServers = computed(() =>
  trustedServers.value.filter(
    (c) =>
      !c.client ||
      c.client?.name
        .toLocaleLowerCase()
        .includes(filterText.value.toLocaleLowerCase())
  )
);
const allClients = shallowRef<IClient[]>([]);

const { pageSize, pageStart, pageEnd, handlePagingUpdate } = usePaging();

async function loadTrustedServers() {
  try {
    status.value = AsyncStatus.BUSY;
    const [clients, servers, accounts] = await Promise.all([
      adminService.getClients(),
      adminService.getTrustedServers(),
      AccountStore.fetchAllAccounts()
    ]);
    allClients.value = clients;
    trustedServers.value = [];
    servers.forEach((s) => {
      const client = clients.find((c) => c.id === s.id);
      const owner = accounts.find((a) => a.ID === s.ownerID);
      if (client && owner) {
        const server = {
          ...s,
          client,
          owner
        };
        trustedServers.value.push(server);
      }
    });
    status.value = AsyncStatus.OK;
  } catch (err: unknown) {
    status.value = AsyncStatus.ERROR;
    console.error('Error loading trustedServers', err);
  }
}

function handleUpdated(trustedServer: IGridTrustedServer) {
  trustedServer.editing = false;
  loadTrustedServers();
}

async function handleDelete(trustedServer: IGridTrustedServer) {
  const confirmDelete = confirm(
    `Are you sure you want to delete server ${trustedServer.client?.name}?`
  );
  if (confirmDelete) {
    try {
      status.value = AsyncStatus.BUSY;
      await adminService.deleteTrustedServer(trustedServer.id);
      loadTrustedServers();
      status.value = AsyncStatus.OK;
    } catch (err: unknown) {
      status.value = AsyncStatus.ERROR;
      console.error('Error deleting trustedServer', err);
    }
  }
}
</script>
