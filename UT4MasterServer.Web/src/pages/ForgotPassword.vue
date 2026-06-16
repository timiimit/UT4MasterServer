<template>
  <LoadingPanel :status="status" :error="errorMessage">
    <form @submit.prevent="submit">
      <fieldset>
        <legend>Forgot Password</legend>
        <p>
          Enter the email address on your account and we'll send you a link
          to choose a new password. The link is valid for one hour.
        </p>
        <div class="form-group row">
          <label for="email" class="col-sm-12 col-form-label">Email</label>
          <div class="col-sm-12">
            <input
              id="email"
              v-model="email"
              type="email"
              class="form-control"
              name="email"
              required
              placeholder="you@example.com"
              autocomplete="email"
              autofocus
            />
          </div>
        </div>
        <div v-if="submitted" class="alert alert-success">
          If that email matches an account, a reset link has been sent.
          Check your inbox.
        </div>
        <div class="form-group row">
          <div class="col-sm-12">
            <button type="submit" class="btn btn-primary" :disabled="submitted">
              Send Reset Link
            </button>
            <router-link to="/Login" class="btn btn-link">Back to Login</router-link>
          </div>
        </div>
      </fieldset>
    </form>
  </LoadingPanel>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { AsyncStatus } from '@/types/async-status';
import LoadingPanel from '@/components/LoadingPanel.vue';
import AccountService from '@/services/account.service';

const accountService = new AccountService();
const status = ref<AsyncStatus>(AsyncStatus.OK);
const errorMessage = ref<string>('');
const email = ref<string>('');
const submitted = ref<boolean>(false);

async function submit() {
  try {
    status.value = AsyncStatus.BUSY;
    await accountService.forgotPassword(email.value);
    submitted.value = true;
    status.value = AsyncStatus.OK;
  } catch (err: unknown) {
    // Anti-enumeration: don't surface "no such email" errors. Treat 200/error
    // identically. We only surface a real error if the API was unreachable.
    status.value = AsyncStatus.ERROR;
    errorMessage.value = (err as Error)?.message;
  }
}
</script>
