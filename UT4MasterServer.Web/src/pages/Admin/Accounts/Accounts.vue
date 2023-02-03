<template>
  <CrudPage title="Accounts">
    <template #filters>
      <div>
        <input
          v-model="filterText"
          type="text"
          class="form-control"
          placeholder="Filter by Username..."
          @keyup="filtersUpdated()"
        />
      </div>
      <div>
        <RoleMultiSelect v-model="filterRoles" />
      </div>
    </template>
    <LoadingPanel :status="status" auto-load @load="searchAccounts">
      <table class="table">
        <thead>
          <tr>
            <th>Username</th>
            <th>Roles</th>
            <th />
          </tr>
        </thead>
        <tbody>
          <template v-for="account in accounts" :key="objectHash(account)">
            <tr :class="{ 'table-light': account.editing }">
              <td class="username">{{ account.username }}</td>
              <td>{{ account.roles?.join(', ') }}</td>
              <td class="actions">
                <button
                  class="btn btn-icon"
                  @click="account.editing = !account.editing"
                >
                  <FontAwesomeIcon icon="fa-regular fa-pen-to-square" />
                </button>
                <button
                  v-if="canDelete(account)"
                  class="btn btn-icon"
                  @click="handleDelete(account)"
                >
                  <FontAwesomeIcon icon="fa-solid fa-trash-can" />
                </button>
              </td>
            </tr>
            <tr v-if="account.editing" class="edit-row table-light">
              <td colspan="3">
                <EditAccount
                  :account="account"
                  @updated="handleUpdated(account)"
                />
              </td>
            </tr>
          </template>
        </tbody>
      </table>
      <Paging
        :item-count="totalAccounts"
        :page-size="pageSize"
        @update="handlePagingUpdate"
      />
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
import { shallowRef, ref, watch } from 'vue';
import CrudPage from '@/components/CrudPage.vue';
import AccountService from '@/services/account.service';
import { IAccountWithRoles } from '@/types/account';
import LoadingPanel from '@/components/LoadingPanel.vue';
import { AsyncStatus } from '@/types/async-status';
import EditAccount from './components/EditAccount.vue';
import { objectHash } from '@/utils/utilities';
import { usePaging } from '@/hooks/use-paging.hook';
import Paging from '@/components/Paging.vue';
import { AccountStore } from '@/stores/account-store';
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';
import { Role } from '@/enums/role';
import AdminService from '@/services/admin-service';
import RoleMultiSelect from './components/RoleMultiSelect.vue';
import { debounce } from 'ts-debounce';

interface IGridAccount extends IAccountWithRoles {
  editing?: boolean;
}

const accountService = new AccountService();
const adminService = new AdminService();
const accounts = ref<IGridAccount[]>([]);
const totalAccounts = shallowRef(0);
const status = shallowRef(AsyncStatus.OK);
const filterText = shallowRef('');
const filterRoles = shallowRef<Role[]>([]);

const { pageSize, pageStart, pageEnd, handlePagingUpdate } = usePaging();

async function searchAccounts() {
  try {
    status.value = AsyncStatus.BUSY;
    const response = await accountService.searchAccounts<IAccountWithRoles>(
      filterText.value,
      pageStart.value,
      pageEnd.value,
      true,
      filterRoles.value
    );
    accounts.value = response.accounts;
    totalAccounts.value = response.count;
    status.value = AsyncStatus.OK;
  } catch (err: unknown) {
    status.value = AsyncStatus.ERROR;
    console.error('Error loading accounts', err);
  }
}

function canDelete(account: IGridAccount) {
  const accountIsUser = AccountStore.account?.id === account.id;
  const accountIsAdmin = account.roles?.includes(Role.Admin);
  const accountIsModerator = account.roles?.includes(Role.Moderator);
  const userIsModerator = AccountStore.account?.roles?.includes(Role.Moderator);

  return (
    !accountIsUser &&
    !accountIsAdmin &&
    !(userIsModerator && (accountIsAdmin || accountIsModerator))
  );
}

async function handleDelete(account: IGridAccount) {
  // TODO: something less hideous than browser confirm dialog
  const confirmDelete = confirm(
    `Are you sure you want to delete account ${account.username}?`
  );
  if (confirmDelete) {
    try {
      status.value = AsyncStatus.BUSY;
      await adminService.deleteAccount(account.id);
      searchAccounts();
      status.value = AsyncStatus.OK;
    } catch (err: unknown) {
      status.value = AsyncStatus.ERROR;
      console.error('Error deleting account', err);
    }
  }
}

function handleUpdated(account: IGridAccount) {
  account.editing = false;
  searchAccounts();
}

const filtersUpdated = debounce(searchAccounts, 500);

watch(filterRoles, searchAccounts);
watch(pageEnd, searchAccounts);
</script>
