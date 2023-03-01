<template>
  <LoadingPanel
    :status="status"
    :error="errorMessage"
    auto-load
    @load="activateAccount"
  >
    <div v-show="activated" class="alert alert-dismissible alert-success">
      <div>
        Account activated successfully.
        <span v-if="!SessionStore.isAuthenticated"
          >Click <a href="/Login">here</a> to go to the login page.</span
        >
      </div>
    </div>
  </LoadingPanel>
</template>

<script setup lang="ts">
import { shallowRef } from 'vue';
import { useRoute } from 'vue-router';
import LoadingPanel from '@/components/LoadingPanel.vue';
import { AsyncStatus } from '@/types/async-status';
import AccountService from '@/services/account.service';
import { SessionStore } from '@/stores/session-store';

const status = shallowRef(AsyncStatus.OK);
const errorMessage = shallowRef('Error occurred while activating account.');
const activated = shallowRef(false);
const route = useRoute();
const accountService = new AccountService();

async function activateAccount() {
  try {
    status.value = AsyncStatus.BUSY;
    const { accountId, guid } = route.query;

    if (!accountId?.length || !guid?.length) {
      status.value = AsyncStatus.ERROR;
      return;
    }

    const formData = {
      accountId: accountId as string,
      guid: guid as string
    };

    await accountService.activateAccount(formData);
    status.value = AsyncStatus.OK;
    activated.value = true;
  } catch (err: unknown) {
    status.value = AsyncStatus.ERROR;
    errorMessage.value = (err as Error)?.message;
  }
}
</script>
