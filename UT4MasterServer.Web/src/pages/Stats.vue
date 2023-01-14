<template>
  <h1>Stats</h1>
  <LoadingPanel :status="accountsStatus">
    <template #default>
      <div class="form-group row">
        <div class="col-sm-6">
          <label for="accountId" class="col-sm-6 col-form-label">Account</label>
          <select class="form-select" v-model="accountId" @change="handleParameterChange">
            <option :value="AccountStore.account?.id">
              {{ AccountStore.account?.displayName }} (You)
            </option>
            <option :value="account.id" v-for="account in accounts">
              {{ account.displayName }}
            </option>
          </select>
        </div>
        <div class="col-sm-6">
          <label for="statWindow" class="col-sm-6 col-form-label">Timeframe</label>
          <select class="form-select" v-model="statWindow" @change="handleParameterChange">
            <option :value="window.value" v-for="window in statWindowOptions">
              {{ window.text }}
            </option>
          </select>
        </div>
      </div>
    </template>
    <template #error>
      Error loading user accounts. Please try again.
    </template>
  </LoadingPanel>
  <LoadingPanel :status="statsStatus">
    <template #default>
      <h3>Overall Stats</h3>
      <div class="row">
        <div class="col-sm-3">
          <div class="card bg-light">
            <div class="card-header">Quick Look</div>
            <div class="card-body">
              <ul class="card-text">
                <li>Duel MMR: <span>Unranked</span></li>
              </ul>
            </div>
          </div>
        </div>
      </div>
    </template>
    <template #error>
      Error loading stats. Please try again.
    </template>
  </LoadingPanel>
</template>

<script setup lang="ts">
import LoadingPanel from '../components/LoadingPanel.vue';
import { AsyncStatus } from '../types/async-status';
import { shallowRef, onMounted } from 'vue';
import { SessionStore } from '../stores/session-store';
import StatsService from '../services/stats.service';
import { StatisticWindow } from '../enums/statistic-window';
import { IAccount } from '../types/account';
import AccountService from '../services/account.service';
import { AccountStore } from '../stores/account-store';

const statsStatus = shallowRef(AsyncStatus.OK);
const accountsStatus = shallowRef(AsyncStatus.OK);
const accountId = shallowRef(SessionStore.session?.account_id ?? '');
const statWindow = shallowRef(StatisticWindow.AllTime);
const accounts = shallowRef<IAccount[]>([]);

const statsService = new StatsService();
const accountService = new AccountService();

const statWindowOptions = [
  { text: 'All Time', value: StatisticWindow.AllTime },
  { text: 'Daily', value: StatisticWindow.Daily },
  { text: 'Weekly', value: StatisticWindow.Weekly },
  { text: 'Monthly', value: StatisticWindow.Monthly }
];

async function loadStats() {
  try {
    statsStatus.value = AsyncStatus.BUSY;
    console.debug('Load Stats: ', accountId.value, statWindow.value);
    const response = await statsService.getStats(accountId.value, statWindow.value);
    console.debug('Stats Response', response);
    statsStatus.value = AsyncStatus.OK;
  }
  catch (err: unknown) {
    statsStatus.value = AsyncStatus.ERROR;
    console.error(err);
  }
}

async function loadAccounts() {
  try {
    accountsStatus.value = AsyncStatus.BUSY;
    const response = await accountService.getAllAccounts();
    accounts.value = response.filter((a) => a.id !== AccountStore.account?.id);
    accountsStatus.value = AsyncStatus.OK;
  }
  catch (err: unknown) {
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

onMounted(async () => {
  await loadAccounts();
  loadStats();
});

</script>
