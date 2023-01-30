<template>
  <div class="navbar navbar-primary bg-light user-info">
    <div class="container">
      <div>
        <label>
          <RouterLink to="/Profile/PlayerCard">Username:</RouterLink>
        </label> {{ AccountStore.account?.Username ?? SessionStore.username }}
      </div>
      <div>
        <LoadingPanel :status="status">
          <label>Auth Code: </label>
          <span v-if="authCode">{{ authCode }}</span>
          <button type="button" class="btn btn-sm btn-primary" @click="getAuthCode">Get Code</button>
          <CopyButton v-if="authCode" :subject="authCode" custom-class="btn-secondary" />
        </LoadingPanel>
      </div>
    </div>
  </div>
</template>

<style lang="scss" scoped>
.user-info {
  padding: 10px 0;
  text-transform: none;
  font-size: 0.8rem;

  label {
    text-transform: uppercase;
    font-size: 0.6rem;
    margin-right: 5px;
  }

  button {
    margin-left: 5px;
    padding: 3px 5px;
  }

  a {
    text-decoration: none;
  }
}
</style>

<script setup lang="ts">
import { shallowRef, onMounted } from 'vue';
import AuthenticationService from '@/services/authentication.service';
import { AsyncStatus } from '@/types/async-status';
import LoadingPanel from './LoadingPanel.vue';
import { AccountStore } from '@/stores/account-store';
import { SessionStore } from '@/stores/session-store';
import CopyButton from './CopyButton.vue';

const service = new AuthenticationService();
const authCode = shallowRef<string | null>(null);
const status = shallowRef(AsyncStatus.OK);

async function getAuthCode() {
  try {
    status.value = AsyncStatus.BUSY;
    authCode.value = await service.getAuthCode();
    status.value = AsyncStatus.OK;
  }
  catch (err: unknown) {
    status.value = AsyncStatus.ERROR;
  }
}

onMounted(() => {
  if (AccountStore.account === null) {
    AccountStore.fetchUserAccount();
  }
})

</script>
