<template>
  <LoadingPanel :status="status">
    <template #default>
      <form @keydown.enter="logIn">
        <fieldset>
          <legend>Log In</legend>
          <div class="form-group row">
            <label for="username" class="col-sm-12 col-form-label">Username</label>
            <div class="col-sm-6">
              <input type="text" class="form-control" id="username" name="username" required v-model="username"
                placeholder="Username" autocomplete="username" />
              <div class="invalid-feedback">Username is required</div>
            </div>
            <div class="col-sm-6 flex-v-center">
              <div class="form-check">
                <input class="form-check-input" type="checkbox" value="" id="saveUsername" v-model="saveUsername">
                <label class="form-check-label" for="saveUsername">
                  Save Username
                </label>
              </div>
            </div>
          </div>
          <div class="form-group row">
            <label for="password" class="col-sm-12 col-form-label">Password</label>
            <div class="col-sm-6">
              <input type="password" class="form-control" id="password" name="password" minlength="7" v-model="password"
                placeholder="Password" autocomplete="current-password" />
              <div class="invalid-feedback">Password must be at least 7 characters</div>
            </div>
          </div>
          <div class="form-group row">
            <div class="col-sm-12">
              <button type="button" class="btn btn-primary" :disabled="!formValid" @click="logIn">Log In</button>
            </div>
          </div>
        </fieldset>
      </form>
    </template>
    <template #error>
      Error logging in. Please try again.
    </template>
  </LoadingPanel>
  <RouterLink to="/Register">Create an account</RouterLink>
</template>

<style>
.form-group {
  margin-bottom: 1rem;
}

.flex-v-center {
  display: flex;
  align-items: center;
}
</style>

<script setup lang="ts">
import LoadingPanel from '../components/LoadingPanel.vue';
import { AsyncStatus } from '../types/async-status';
import { computed, ref, shallowRef } from 'vue';
import CryptoJS from 'crypto-js';
import AuthenticationService from '../services/authentication.service';
import { UserStore } from '../stores/user-store';
import { useRouter } from 'vue-router';

const username = shallowRef(UserStore.username ?? '');
const password = shallowRef('');
const saveUsername = shallowRef(UserStore.saveUsername);
const status = ref(AsyncStatus.OK);
const formValid = computed(() => username.value && password.value.length > 7);

const authenticationService = new AuthenticationService();

const router = useRouter();

async function logIn() {
  try {
    status.value = AsyncStatus.BUSY;
    const formData = {
      username: username.value,
      password: CryptoJS.SHA512(password.value).toString(),
      grant_type: 'password'
    };
    UserStore.saveUsername = saveUsername.value;
    console.debug('Log in request', formData);
    await authenticationService.logIn(formData);
    status.value = AsyncStatus.OK;
    router.push('/Profile/Stats');
  }
  catch (err: unknown) {
    status.value = AsyncStatus.ERROR;
    console.error(err);
  }
}
</script>
