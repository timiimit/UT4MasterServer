<template>
  <LoadingPanel
    :status="status"
    :error="errorMessage"
    auto-load
    @load="parseQueryValues"
  >
    <form @submit.prevent="resetPassword">
      <fieldset>
        <legend>Reset Password</legend>
        <div
          v-show="passwordChanged"
          class="alert alert-dismissible alert-success"
        >
          <div>
            Password changed successfully. Click
            <RouterLink to="/Login">here</RouterLink> to go to login page.
          </div>
        </div>
        <div v-show="!passwordChanged" class="form-group row">
          <label for="newPassword" class="col-sm-12 col-form-label"
            >New Password</label
          >
          <div class="col-sm-6">
            <input
              id="newPassword"
              v-model="newPassword"
              type="password"
              class="form-control"
              name="newPassword"
              required
              placeholder="New Password"
              autofocus
            />
            <div class="invalid-feedback">New password is required</div>
          </div>
        </div>
        <div v-show="!passwordChanged" class="form-group row">
          <label for="confirmNewPassword" class="col-sm-12 col-form-label"
            >Confirm New Password</label
          >
          <div class="col-sm-6">
            <input
              id="confirmNewPassword"
              v-model="confirmNewPassword"
              type="password"
              class="form-control"
              name="confirmNewPassword"
              required
              placeholder="Confirm New Password"
              autofocus
            />
            <div class="invalid-feedback">Confirm new password is required</div>
          </div>
        </div>
        <div v-show="!passwordChanged" class="form-group row">
          <div class="col-sm-12">
            <button
              type="submit"
              class="btn btn-primary"
              :disabled="!formValid"
              tabindex="2"
            >
              Change password
            </button>
          </div>
        </div>
      </fieldset>
    </form>
  </LoadingPanel>
</template>

<script setup lang="ts">
import { shallowRef, computed } from 'vue';
import { useRoute } from 'vue-router';
import CryptoJS from 'crypto-js';
import LoadingPanel from '@/components/LoadingPanel.vue';
import { AsyncStatus } from '@/types/async-status';
import { validatePassword } from '@/utils/validation';
import AccountService from '@/services/account.service';

const route = useRoute();
const status = shallowRef(AsyncStatus.OK);
const passwordChanged = shallowRef(false);
const errorMessage = shallowRef('Error occurred while resetting password.');
const accountId = shallowRef('');
const guid = shallowRef('');
const newPassword = shallowRef('');
const confirmNewPassword = shallowRef('');
const formValid = computed(
  () =>
    newPassword.value &&
    validatePassword(newPassword.value) &&
    confirmNewPassword.value &&
    newPassword.value === confirmNewPassword.value
);
const accountService = new AccountService();

async function parseQueryValues() {
  const { accountId: qAccountId, guid: qGuid } = route.query;

  if (
    qAccountId === undefined ||
    qGuid === undefined ||
    qAccountId?.toString() === '' ||
    qGuid?.toString() === ''
  ) {
    status.value = AsyncStatus.ERROR;
    errorMessage.value = 'Bad request';
    return;
  }

  accountId.value = qAccountId as string;
  guid.value = qGuid as string;
}

async function resetPassword() {
  try {
    status.value = AsyncStatus.BUSY;

    const formData = {
      accountId: accountId.value,
      guid: guid.value,
      newPassword: CryptoJS.SHA512(newPassword.value).toString()
    };

    await accountService.resetPassword(formData);

    status.value = AsyncStatus.OK;
    passwordChanged.value = true;
  } catch (err: unknown) {
    passwordChanged.value = false;
    status.value = AsyncStatus.ERROR;
    errorMessage.value = (err as Error)?.message;
  }
}
</script>
