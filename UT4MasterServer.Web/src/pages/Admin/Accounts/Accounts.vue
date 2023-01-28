<template>
    <CrudPage title="Accounts">
        <template #add="p">
            <h2>Add Account</h2>
            <button class="btn btn-sm btn-secondary" @click="p.cancel">Cancel</button>
        </template>
        <template #filters>
            <input type="text" class="form-control" placeholder="Search..." v-model="filterText" />
        </template>
        <LoadingPanel :status="status" @load="loadAccounts" auto-load>
            <table class="table">
                <thead>
                    <tr>
                        <th>Username</th>
                        <th />
                    </tr>
                </thead>
                <tbody>
                    <!-- TODO: add paging -->
                    <template v-for="account in filteredAccounts.slice(0, 50)" :key="objectHash(account)">
                        <tr :class="{'table-light': account.editing }">
                            <td>{{ account.Username }}</td>
                            <td width="10%">
                                <button class="btn btn-sm btn-smaller btn-primary edit-button"
                                    @click="account.editing = !account.editing">{{
                                        account.editing ?
                                            'Cancel' : 'Edit'
                                    }}</button>
                            </td>
                        </tr>
                        <tr v-if="account.editing" class="edit-row table-light">
                            <td colspan="100">
                                <EditAccount :account="account" />
                            </td>
                        </tr>
                    </template>
                </tbody>
            </table>
        </LoadingPanel>
    </CrudPage>
</template>

<style lang="scss" >
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

interface IGridAccount extends IAccount {
    editing?: boolean;
}

const accountService = new AccountService();
const accounts = ref<IGridAccount[]>([]);
const status = shallowRef(AsyncStatus.OK);
const filterText = shallowRef('');

const filteredAccounts = computed(() => accounts.value.filter((a) => a.Username.toLocaleLowerCase().includes(filterText.value.toLocaleLowerCase())));

async function loadAccounts() {
    try {
        status.value = AsyncStatus.BUSY;
        accounts.value = await accountService.getAllAccounts();
        status.value = AsyncStatus.OK;
    } catch (err: unknown) {
        status.value = AsyncStatus.ERROR;
        console.error('Error loading accounts', err);
    }
}

</script>