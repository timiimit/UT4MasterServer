<template>
  <h1>Stats</h1>

  <div class="form-group row">
    <div class="col-sm-6">
      <LoadingPanel :status="accountsStatus">
        <label for="accountId" class="col-sm-6 col-form-label">Account</label>
        <Autocomplete
          v-if="AccountStore.account"
          :value="accountId"
          :items="accounts"
          item-key="id"
          search-key="username"
          @input-change="searchAccounts"
          @select="handleSelectAccount"
        />
      </LoadingPanel>
    </div>
    <div class="col-sm-6">
      <label for="statWindow" class="col-sm-6 col-form-label">
        Timeframe
      </label>
      <select
        v-model="statWindow"
        class="form-select"
        @change="handleParameterChange"
      >
        <option
          v-for="window in statWindowOptions"
          :key="window.value"
          :value="window.value"
        >
          {{ window.text }}
        </option>
      </select>
    </div>
  </div>
  <LoadingPanel :status="statsStatus">
    <template v-if="accountId">
      <h5>Viewing stats for: {{ viewingAccount?.username }}</h5>
      <StatSection
        v-for="section in statSections"
        :key="section.heading"
        :data="stats"
        :section="section"
      />
    </template>
    <h5 v-else class="text-center">Select a player to view stats</h5>
  </LoadingPanel>
</template>

<script setup lang="ts">
import LoadingPanel from '@/components/LoadingPanel.vue';
import { AsyncStatus } from '@/types/async-status';
import { shallowRef, ref, onMounted, computed } from 'vue';
import StatsService from '@/services/stats.service';
import { StatisticWindow } from '@/enums/statistic-window';
import { IAccount } from '@/types/account';

import StatSection from '@/pages/Stats/components/StatSection.vue';
import { IStatisticData } from '@/types/statistic-data';
import Autocomplete from '@/components/Autocomplete.vue';
import { AccountStore } from '@/stores/account-store';
import AccountService from '@/services/account.service';
import { useRoute } from 'vue-router';
import { statSections } from './data/statistics-config';

const route = useRoute();
const statsStatus = shallowRef(AsyncStatus.OK);
const statWindow = shallowRef(StatisticWindow.AllTime);
const stats = shallowRef<IStatisticData[]>([]);

const accountsStatus = shallowRef(AsyncStatus.OK);
const accountId = ref<string | undefined>(
  route.params.accountId?.length
    ? (route.params.accountId as string)
    : undefined
);
const accounts = ref<IAccount[]>([]);
const viewingAccount = computed(() =>
  accounts.value.find((a) => a?.id === accountId.value)
);

const statsService = new StatsService();
const accountService = new AccountService();

const statWindowOptions = [
  { text: 'All Time', value: StatisticWindow.AllTime },
  { text: 'Daily', value: StatisticWindow.Daily },
  { text: 'Weekly', value: StatisticWindow.Weekly },
  { text: 'Monthly', value: StatisticWindow.Monthly }
];

async function loadStats() {
  if (!accountId.value) {
    return;
  }
  try {
    statsStatus.value = AsyncStatus.BUSY;
    stats.value = await statsService.getStats(
      accountId.value,
      statWindow.value
    );
    statsStatus.value = AsyncStatus.OK;
  } catch (err: unknown) {
    statsStatus.value = AsyncStatus.ERROR;
    console.error(err);
  }
}

async function searchAccounts(query: string) {
  try {
    accountsStatus.value = AsyncStatus.BUSY;
    accounts.value = (await accountService.searchAccounts(query)).accounts;
    // remove logged in user from search results
    accounts.value = accounts.value.filter(
      (a) => a.id !== AccountStore.account?.id
    );
    // always show logged in user as first option
    if (AccountStore.account) {
      accounts.value.unshift(AccountStore.account);
    }
    if (!accountId.value?.length) {
      accountId.value = AccountStore.account?.id;
    }
    accountsStatus.value = AsyncStatus.OK;
  } catch (err: unknown) {
    accountsStatus.value = AsyncStatus.ERROR;
    console.error(err);
  }
}

function handleParameterChange() {
  if (!accountId.value || !statWindow.value) {
    return;
  }
  loadStats();
}

function handleSelectAccount(account: IAccount) {
  accountId.value = account?.id;
  window.history.pushState({}, '', `/Stats/${account.id}`);
  handleParameterChange();
}

async function setCurrentAccount() {
  let account: IAccount | undefined = undefined;
  // if account is passed as param
  if (accountId.value?.length && accountId.value !== AccountStore.account?.id) {
    account = (await accountService.getAccountsByIds([accountId.value]))[0];
  }
  // if no account passed as param, use logged in account (if available)
  if (!account && AccountStore.account) {
    account = AccountStore.account;
  }
  if (!account) {
    return;
  }
  accounts.value = [account];
  handleSelectAccount(account);
}

onMounted(() => {
  setCurrentAccount();
  loadStats();
});
</script>
