<template>
  <LoadingPanel
    :status="status"
    :error="errorMessage"
    auto-load
    @load="verifyEmail"
  >
    <div v-show="verified" class="alert alert-dismissible alert-success">
      <div>
        Email verified successfully.
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
const errorMessage = shallowRef('Error occurred while verifying email.');
const verified = shallowRef(false);
const route = useRoute();
const accountService = new AccountService();

async function verifyEmail() {
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

    await accountService.verifyEmail(formData);

    status.value = AsyncStatus.OK;
    verified.value = true;
  } catch (err: unknown) {
    status.value = AsyncStatus.ERROR;
    errorMessage.value = (err as Error)?.message;
  }
}
</script>
