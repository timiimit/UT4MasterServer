<template>
  <LoadingPanel :status="status">
    <template #default>
      <form :class="{ 'was-validated': submitAttempted }" @submit.prevent="handleSubmit" novalidate>
        <fieldset>
          <legend>Change Email</legend>
          <div class="form-group row">
            <label for="currentEmail" class="col-sm-12 col-form-label">Current Email</label>
            <div class="col-sm-6">
              <input type="email" class="form-control" id="currentEmail" name="currentEmail" v-model="currentEmail"
                readonly disabled />
            </div>
          </div>
          <div class="form-group row">
            <label for="newEmail" class="col-sm-12 col-form-label">New Email</label>
            <div class="col-sm-6">
              <input type="email" placeholder="player@example.com" class="form-control" id="newEmail" name="newEmail"
                v-model="newEmail" required />
              <div class="invalid-feedback">A valid email address is required</div>
            </div>
          </div>
          <div class="form-group row">
            <div class="col-sm-12">
              <button type="submit" class="btn btn-primary">Change Email</button>
            </div>
          </div>
        </fieldset>
      </form>
    </template>
    <template #error>
      Error changing email. Please try again.
    </template>
  </LoadingPanel>
</template>

<script setup lang="ts">
import { IChangeEmailRequest } from '../types/change-email-request';
import { shallowRef, computed } from 'vue';
import AccountService from '../services/account.service';
import { AsyncStatus } from '../types/async-status';
import { useRouter } from 'vue-router';
import LoadingPanel from '../components/LoadingPanel.vue';
import { AccountStore } from '../stores/account-store';

const accountService = new AccountService();
const router = useRouter();

const status = shallowRef(AsyncStatus.OK);
const currentEmail = shallowRef(AccountStore.account?.email);
const newEmail = shallowRef<string>('');
const submitAttempted = shallowRef(false);

const formValid = computed(() => newEmail.value?.length);

async function handleSubmit() {
  submitAttempted.value = true;
  if (!formValid.value) { return; }

  try {
    status.value = AsyncStatus.BUSY;
    const request: IChangeEmailRequest = {
      newEmail: newEmail.value
    };
    console.debug('Change username request', request);
    await accountService.changeEmail(request);
    status.value = AsyncStatus.OK;
    AccountStore.fetchUserAccount();
    router.push('/Profile');
  }
  catch (err: unknown) {
    status.value = AsyncStatus.ERROR;
    console.error(err);
  }
}
</script>

