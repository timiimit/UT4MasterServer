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

const username = shallowRef(localStorage.getItem('ut4uu_username') ?? '');
const password = shallowRef('');
const saveUsername = shallowRef(!!localStorage.getItem('ut4uu_username'));
const status = ref(AsyncStatus.OK);
const formValid = computed(() => username.value && password.value.length > 7);

const authenticationService = new AuthenticationService();

async function logIn() {
  try {
    status.value = AsyncStatus.BUSY;
    const formData = {
      username: username.value,
      password: CryptoJS.SHA512(password.value).toString(),
      grant_type: 'password'
    };
    if (saveUsername.value) {
      localStorage.setItem('ut4uu_username', username.value!);
    } else {
      localStorage.removeItem('ut4uu_username');
    }
    console.debug('log in', formData);
    await authenticationService.logIn(formData);
    status.value = AsyncStatus.OK;
  }
  catch (err: unknown) {
    status.value = AsyncStatus.ERROR;
    console.error(err);
  }
}
</script>