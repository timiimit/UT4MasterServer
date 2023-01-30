<template>
    <CrudPage title="Clients">
        <template #add="p">
            <AddClient :all-clients="clients" @cancel="p.cancel" @added="loadClients(); p.cancel();" />
        </template>
        <template #filters>
            <input type="text" class="form-control" placeholder="Search..." v-model="filterText" />
        </template>
        <LoadingPanel :status="status" @load="loadClients" auto-load>
            <table class="table">
                <thead>
                    <tr>
                        <th>Name</th>
                        <th>ID</th>
                        <th />
                    </tr>
                </thead>
                <tbody>
                    <template v-for="client in filteredClients.slice(pageStart, pageEnd)" :key="objectHash(client)">
                        <tr :class="{ 'table-light': client.editing }">
                            <td>{{ client.name }}</td>
                            <td>{{ client.id }}</td>
                            <td class="actions">
                                <button class="btn btn-icon" @click="client.editing = !client.editing">
                                    <FontAwesomeIcon icon="fa-regular fa-pen-to-square" />
                                </button>
                                <button v-if="canDelete(client)" class="btn btn-icon" @click="handleDelete(client)">
                                    <FontAwesomeIcon icon="fa-solid fa-trash-can" />
                                </button>
                            </td>
                        </tr>
                        <tr v-if="client.editing" class="edit-row table-light">
                            <td colspan="3">
                                <EditClient :client="client" @cancel="client.editing = false"
                                    @updated="handleUpdated(client)" />
                            </td>
                        </tr>
                    </template>
                </tbody>
            </table>
            <Paging :items="filteredClients" :page-size="pageSize" @update="handlePagingUpdate" />
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
import { IClient } from './types/client';
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';
import AddClient from './components/AddClient.vue';
import EditClient from './components/EditClient.vue';
import Paging from '@/components/Paging.vue';
import { usePaging } from '@/hooks/use-paging.hook';
import { useClientOptions } from './hooks/use-client-options.hook';

interface IGridClient extends IClient {
    editing?: boolean;
}

const adminService = new AdminService();
const clients = ref<IGridClient[]>([]);
const status = shallowRef(AsyncStatus.OK);
const filterText  = shallowRef('');
const filteredClients = computed(() => clients.value.filter((c) => c.name.toLocaleLowerCase().includes(filterText.value.toLocaleLowerCase())));

const { pageSize, pageStart, pageEnd, handlePagingUpdate } = usePaging();
const { isReservedClient } = useClientOptions();

async function loadClients() {
    try {
        status.value = AsyncStatus.BUSY;
        clients.value = await adminService.getClients();
        status.value = AsyncStatus.OK;
    } catch (err: unknown) {
        status.value = AsyncStatus.ERROR;
        console.error('Error loading clients', err);
    }
}

function canDelete(client: IGridClient) {
    return !isReservedClient(client);
}

function handleUpdated(client: IGridClient) {
    client.editing = false;
    loadClients();
}

async function handleDelete(client: IGridClient) {
    const confirmDelete = confirm(`Are you sure you want to delete client ${client.name}?`);
    if (confirmDelete) {
        try {
            status.value = AsyncStatus.BUSY;
            await adminService.deleteClient(client.id);
            loadClients();
            status.value = AsyncStatus.OK;
        } catch (err: unknown) {
            status.value = AsyncStatus.ERROR;
            console.error('Error deleting client', err);
        }
    }
}

</script>
