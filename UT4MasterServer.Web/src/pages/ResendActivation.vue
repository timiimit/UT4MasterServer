<template>
  <LoadingPanel :status="status" :error="errorMessage">
    <form @submit.prevent="resendActivationLink">
      <fieldset>
        <legend>Resend Activation Link</legend>
        <div
          v-show="activationLinkSent"
          class="alert alert-dismissible alert-success"
        >
          <div>Activation link sent to email.</div>
        </div>
        <div class="form-group row">
          <label for="email" class="col-sm-12 col-form-label">Email</label>
          <div class="col-sm-6">
            <input
              id="username"
              v-model="email"
              type="text"
              class="form-control"
              name="email"
              required
              placeholder="Email"
              autocomplete="email"
              autofocus
            />
            <div class="invalid-feedback">Email is required</div>
          </div>
        </div>
        <div class="form-group row">
          <div class="col-sm-12">
            <button
              type="submit"
              class="btn btn-primary"
              :disabled="!formValid"
              tabindex="2"
            >
              Resend activation link
            </button>
          </div>
        </div>
      </fieldset>
    </form>
  </LoadingPanel>
</template>

<script setup lang="ts">
import { shallowRef, computed } from 'vue';
import LoadingPanel from '@/components/LoadingPanel.vue';
import { AsyncStatus } from '@/types/async-status';
import { validateEmail } from '@/utils/validation';
import AccountService from '@/services/account.service';

const status = shallowRef(AsyncStatus.OK);
const errorMessage = shallowRef(
  'Error occurred while sending reset password link.'
);
const email = shallowRef('');
const formValid = computed(() => email.value && validateEmail(email.value));
const activationLinkSent = shallowRef(false);
const accountService = new AccountService();

async function resendActivationLink() {
  try {
    status.value = AsyncStatus.BUSY;

    await accountService.resendActivationLink(email.value);
    status.value = AsyncStatus.OK;
    activationLinkSent.value = true;
  } catch (err: unknown) {
    activationLinkSent.value = false;
    status.value = AsyncStatus.ERROR;
    errorMessage.value = (err as Error)?.message;
  }
}
</script>
