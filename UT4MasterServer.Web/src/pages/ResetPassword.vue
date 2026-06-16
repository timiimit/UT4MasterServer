<template>
  <LoadingPanel :status="status" :error="errorMessage">
    <form @submit.prevent="submit">
      <fieldset>
        <legend>Reset Password</legend>
        <div v-if="!token" class="alert alert-danger">
          Missing or invalid reset link. Please request a new one.
          <router-link to="/ForgotPassword">Forgot password?</router-link>
        </div>
        <template v-else>
          <div class="form-group row">
            <label for="newPassword" class="col-sm-12 col-form-label">New Password</label>
            <div class="col-sm-12">
              <input
                id="newPassword"
                v-model="newPassword"
                type="password"
                class="form-control"
                required
                minlength="7"
                placeholder="New password"
                autocomplete="new-password"
                autofocus
              />
              <div class="invalid-feedback">
                Password must be at least 7 characters
              </div>
            </div>
          </div>
          <div class="form-group row">
            <label for="confirmPassword" class="col-sm-12 col-form-label">Confirm Password</label>
            <div class="col-sm-12">
              <input
                id="confirmPassword"
                v-model="confirmPassword"
                type="password"
                class="form-control"
                required
                minlength="7"
                placeholder="Confirm new password"
                autocomplete="new-password"
              />
            </div>
          </div>
          <div v-if="success" class="alert alert-success">
            Password reset. <router-link to="/Login">Sign in</router-link>.
          </div>
          <div v-if="mismatch" class="alert alert-danger">Passwords don't match.</div>
          <div class="form-group row">
            <div class="col-sm-12">
              <button type="submit" class="btn btn-primary" :disabled="success">
                Set New Password
              </button>
              <router-link to="/Login" class="btn btn-link">Back to Login</router-link>
            </div>
          </div>
        </template>
      </fieldset>
    </form>
  </LoadingPanel>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import { useRoute } from 'vue-router';
import CryptoJS from 'crypto-js';
import { AsyncStatus } from '@/types/async-status';
import LoadingPanel from '@/components/LoadingPanel.vue';
import AccountService from '@/services/account.service';

const route = useRoute();
const accountService = new AccountService();

const status = ref<AsyncStatus>(AsyncStatus.OK);
const errorMessage = ref<string>('');
const newPassword = ref<string>('');
const confirmPassword = ref<string>('');
const success = ref<boolean>(false);

const token = computed(() => {
  const raw = route.query.token;
  return typeof raw === 'string' ? raw : '';
});

const mismatch = computed(
  () =>
    newPassword.value.length > 0 &&
    confirmPassword.value.length > 0 &&
    newPassword.value !== confirmPassword.value
);

async function submit() {
  if (mismatch.value || !token.value) return;
  try {
    status.value = AsyncStatus.BUSY;
    // The API expects SHA-512 hashed passwords (ValidationHelper.ValidatePassword
    // requires 128 hex chars). Same pattern as Login.vue / Register.vue.
    const hashed = CryptoJS.SHA512(newPassword.value).toString();
    await accountService.resetPassword(token.value, hashed);
    success.value = true;
    status.value = AsyncStatus.OK;
  } catch (err: unknown) {
    status.value = AsyncStatus.ERROR;
    errorMessage.value = (err as Error)?.message;
  }
}
</script>
