<template>
  <LoadingPanel :status="status" :error="errorMessage">
    <form @submit.prevent="logIn">
      <fieldset>
        <legend>Log In</legend>
        <div class="form-group row">
          <label for="username" class="col-sm-12 col-form-label"
            >Username</label
          >
          <div class="col-sm-6">
            <input
              id="username"
              v-model="username"
              type="text"
              class="form-control"
              name="username"
              required
              placeholder="Username"
              autocomplete="username"
              autofocus
            />
            <div class="invalid-feedback">Username is required</div>
          </div>
          <div class="col-sm-6 flex-v-center">
            <div class="form-check">
              <input
                id="saveUsername"
                v-model="saveUsername"
                class="form-check-input"
                type="checkbox"
                value=""
                tabindex="-1"
              />
              <label class="form-check-label" for="saveUsername">
                Save Username
              </label>
            </div>
          </div>
        </div>
        <div class="form-group row">
          <label for="password" class="col-sm-12 col-form-label"
            >Password</label
          >
          <div class="col-sm-6">
            <input
              id="password"
              v-model="password"
              type="password"
              class="form-control"
              name="password"
              minlength="7"
              placeholder="Password"
              autocomplete="current-password"
            />
            <div class="invalid-feedback">
              Password must be at least 7 characters
            </div>
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
              Log In
            </button>
          </div>
        </div>
      </fieldset>
    </form>
  </LoadingPanel>
  <RouterLink to="/Register">Create an account</RouterLink>
</template>

<script setup lang="ts">
import LoadingPanel from '@/components/LoadingPanel.vue';
import { AsyncStatus } from '@/types/async-status';
import { computed, shallowRef } from 'vue';
import CryptoJS from 'crypto-js';
import AuthenticationService from '@/services/authentication.service';
import { SessionStore } from '@/stores/session-store';
import { useRoute, useRouter } from 'vue-router';
import { GrantType } from '@/enums/grant-type';
import { validatePassword } from '@/utils/validation';

const username = shallowRef(SessionStore.username ?? '');
const password = shallowRef('');
const saveUsername = shallowRef(SessionStore.saveUsername);
const status = shallowRef(AsyncStatus.OK);
const formValid = computed(
  () => username.value && validatePassword(password.value)
);
const errorMessage = shallowRef('Error logging in. Please try again.');

const authenticationService = new AuthenticationService();

const router = useRouter();
const route = useRoute();

async function logIn() {
  try {
    status.value = AsyncStatus.BUSY;
    const formData = {
      username: username.value,
      password: CryptoJS.SHA512(password.value).toString(),
      grant_type: GrantType.Password
    };
    SessionStore.saveUsername = saveUsername.value;
    await authenticationService.passwordLogin(formData);
    status.value = AsyncStatus.OK;
    const redirectTo = (route.query.redirect ?? 'Profile') as string;
    router.push(redirectTo);
  } catch (err: unknown) {
    status.value = AsyncStatus.ERROR;
    errorMessage.value = (err as Error)?.message;
  }
}
</script>
