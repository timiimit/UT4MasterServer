<template>
  <LoadingPanel :status="status" :error="errorMessage">
    <form :class="{ 'was-validated': submitAttempted }" @submit.prevent="handleSubmit" novalidate>
      <fieldset>
        <legend>Change Username</legend>
        <div class="form-group row">
          <label for="currentUsername" class="col-sm-12 col-form-label">Current Username</label>
          <div class="col-sm-6">
            <input type="text" class="form-control" id="currentUsername" name="currentUsername"
              v-model="currentUsername" readonly disabled />
          </div>
        </div>
        <div class="form-group row">
          <label for="newUsername" class="col-sm-12 col-form-label">New Username</label>
          <div class="col-sm-6">
            <input type="text" class="form-control" id="newUsername" name="newUsername" placeholder="New Username"
              v-model="newUsername" required />
            <div class="invalid-feedback">New Username is required</div>
          </div>
        </div>
        <div class="form-group row">
          <div class="col-sm-12">
            <button type="submit" class="btn btn-primary">Change Username</button>
          </div>
        </div>
      </fieldset>
    </form>
  </LoadingPanel>
</template>

<script setup lang="ts">
import { IChangeUsernameRequest } from '@/types/change-username-request';
import { shallowRef, computed, onMounted } from 'vue';
import AccountService from '@/services/account.service';
import { useRouter } from 'vue-router';
import { AsyncStatus } from '@/types/async-status';
import LoadingPanel from '@/components/LoadingPanel.vue';
import { AccountStore } from '@/stores/account-store';

const accountService = new AccountService();
const router = useRouter();

const status = shallowRef(AsyncStatus.OK);
const currentUsername = shallowRef(AccountStore.account?.displayName);
const newUsername = shallowRef<string>('');
const submitAttempted = shallowRef(false);
const formValid = computed(() => newUsername.value?.length);
const errorMessage = shallowRef('Error changing username. Please try again.');

async function handleSubmit() {
  submitAttempted.value = true;
  if (!formValid.value) { return; }
  try {
    status.value = AsyncStatus.BUSY;
    const request: IChangeUsernameRequest = {
      newUsername: newUsername.value
    };
    await accountService.changeUsername(request);
    status.value = AsyncStatus.OK;
    AccountStore.fetchUserAccount();
    router.push('/Profile');
  }
  catch (err: unknown) {
    status.value = AsyncStatus.ERROR;
    errorMessage.value = (err as Error)?.message;
  }
}

onMounted(async () => {
  // force account to be up to date before allowing a user to change
  AccountStore.fetchUserAccount();
});
</script>

