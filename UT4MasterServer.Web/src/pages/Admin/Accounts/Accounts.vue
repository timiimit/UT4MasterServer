<template>
    <CrudPage title="Accounts">
        <template #filters>
            <div>
                <input type="text" class="form-control" placeholder="Filter by Username..." v-model="filterText" />
            </div>
            <div>
                <Multiselect placeholder="Filter by Roles..." v-model="filterRoles" :options="flagOptions"
                    mode="tags" />
            </div>
        </template>
        <LoadingPanel :status="status" @load="loadAccounts" auto-load>
            <table class="table">
                <thead>
                    <tr>
                        <th>Username</th>
                        <th>Roles</th>
                        <th />
                    </tr>
                </thead>
                <tbody>
                    <template v-for="account in filteredAccounts.slice(pageStart, pageEnd)" :key="objectHash(account)">
                        <tr :class="{ 'table-light': account.editing }">
                            <td class="username">{{ account.Username }}</td>
                            <td>{{ account.Roles?.join(', ') }}</td>
                            <td class="actions">
                                <button class="btn btn-icon" @click="account.editing = !account.editing">
                                    <FontAwesomeIcon icon="fa-regular fa-pen-to-square" />
                                </button>
                                <button v-if="canDelete(account)" class="btn btn-icon" @click="handleDelete(account)">
                                    <FontAwesomeIcon icon="fa-solid fa-trash-can" />
                                </button>
                            </td>
                        </tr>
                        <tr v-if="account.editing" class="edit-row table-light">
                            <td colspan="3">
                                <EditAccount :account="account" @updated="handleUpdated(account)" />
                            </td>
                        </tr>
                    </template>
                </tbody>
            </table>
            <Paging :items="filteredAccounts" :page-size="pageSize" @update="handlePagingUpdate" />
        </LoadingPanel>
    </CrudPage>
</template>

<style lang="scss" scoped>
td.username {
    width: 30%;
}

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
import AccountService from '@/services/account.service';
import { IAccount } from '@/types/account';
import LoadingPanel from '@/components/LoadingPanel.vue';
import { AsyncStatus } from '@/types/async-status';
import EditAccount from './components/EditAccount.vue';
import { objectHash } from '@/utils/utilities';
import { usePaging } from '@/hooks/use-paging.hook';
import Paging from '@/components/Paging.vue';
import { AccountStore } from '@/stores/account-store';
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';
import { AccountFlag } from '@/enums/account-flag';
import AdminService from '@/services/admin-service';
import Multiselect from '@vueform/multiselect';

interface IGridAccount extends IAccount {
    editing?: boolean;
}

const accountService = new AccountService();
const adminService = new AdminService();
const accounts = ref<IGridAccount[]>([]);
const status = shallowRef(AsyncStatus.OK);
const filterText = shallowRef('');
const filterRoles = shallowRef<AccountFlag[]>([]);

const allFlags = shallowRef<string[]>([]);
const flagOptions = computed(() => allFlags.value.map((f) => ({ label: f, value: f })));

const { pageSize, pageStart, pageEnd, handlePagingUpdate } = usePaging();

const usernameFiltered = computed(() => accounts.value.filter((a) => a.Username.toLocaleLowerCase().includes(filterText.value.toLocaleLowerCase())));

const filteredAccounts = computed(() => usernameFiltered.value.filter((a) => filterRoles.value.length === 0 || a.Roles?.some((r) => filterRoles.value.includes(r))));

async function loadAccounts() {
    try {
        status.value = AsyncStatus.BUSY;
        const [allPossibleFlags, allAccounts] = await Promise.all([adminService.getAccountFlagOptions(), accountService.getAllAccounts()]);
        allFlags.value = allPossibleFlags;
        accounts.value = allAccounts;
        status.value = AsyncStatus.OK;
    } catch (err: unknown) {
        status.value = AsyncStatus.ERROR;
        console.error('Error loading accounts', err);
    }
}

function canDelete(account: IGridAccount) {
    return AccountStore.account?.ID !== account.ID && !account.Roles?.includes(AccountFlag.Admin);
}

async function handleDelete(account: IGridAccount) {
    // TODO: something less hideous than browser confirm dialog
    const confirmDelete = confirm(`Are you sure you want to delete account ${account.Username}?`);
    if (confirmDelete) {
        try {
            status.value = AsyncStatus.BUSY;
            await adminService.deleteAccount(account.ID);
            loadAccounts();
            status.value = AsyncStatus.OK;
        } catch (err: unknown) {
            status.value = AsyncStatus.ERROR;
            console.error('Error deleting account', err);
        }
    }
}

function handleUpdated(account: IGridAccount) {
    account.editing = false;
    loadAccounts();
}

</script>