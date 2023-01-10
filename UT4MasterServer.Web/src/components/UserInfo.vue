<template>
  <div class="navbar navbar-primary bg-light user-info">
    <div class="container">
      <div>
        <label>Logged in as: </label>{{ SessionStore.username }}
      </div>
      <div>
        <LoadingPanel :status="status">
          <label>Auth Code: </label>
          <span v-if="authCode">{{ authCode }}</span>
          <button type="button" class="btn btn-sm btn-primary" @click="getAuthCode">Get Code</button>
          <button v-if="authCode" type="button" class="btn btn-sm btn-secondary" @click="copyAuthCode">Copy</button>
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
}
</style>
  
<script setup lang="ts">
import { SessionStore } from '../stores/session-store';
import { shallowRef } from 'vue';
import AuthenticationService from '../services/authentication.service';
import { AsyncStatus } from '../types/async-status';
import LoadingPanel from './LoadingPanel.vue';

const service = new AuthenticationService();
const authCode = shallowRef<string | undefined>(undefined);
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

function copyAuthCode() {
  if (authCode.value) {
    navigator.clipboard.writeText(authCode.value);
  }
}
</script>