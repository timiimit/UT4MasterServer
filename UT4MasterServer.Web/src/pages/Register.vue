<template>
  <LoadingPanel :status="status">
    <form @keydown.enter="register">
      <fieldset>
        <legend>Register</legend>
        <div class="form-group row">
          <label for="username" class="col-sm-12 col-form-label">Email</label>
          <div class="col-sm-6">
            <input type="email" class="form-control" id="email" name="email" required v-model="email"
              placeholder="Email" autocomplete="email" />
            <div class="invalid-feedback">Email is required</div>
          </div>
        </div>
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
            <button type="button" class="btn btn-primary" :disabled="!formValid" @click="register">Register</button>
          </div>
        </div>
      </fieldset>
    </form>
    <template #error>
      Error registering account. Please try again.
    </template>
  </LoadingPanel>
</template>

<style>
.form-group {
  margin-bottom: 1rem;
}
</style>

<script setup lang="ts">
import LoadingPanel from '../components/LoadingPanel.vue';
import { AsyncStatus } from '../types/async-status';
import { computed, shallowRef } from 'vue';
import AuthenticationService from '../services/authentication.service';
import { validateEmail } from '../utils/validation';

const username = shallowRef('');
const password = shallowRef('');
const email = shallowRef('');
const status = shallowRef(AsyncStatus.OK);
const formValid = computed(() => validateEmail(email.value) && username.value && password.value.length > 7 && status.value != AsyncStatus.BUSY);

const authenticationService = new AuthenticationService();

async function register() {
  try {
    status.value = AsyncStatus.BUSY;

    const formData = {
      email: email.value,
      username: username.value,
      //TODO: sha512?
      password: password.value
    };
    console.debug('register', formData);
    await authenticationService.register(formData);
    status.value = AsyncStatus.OK;
  } catch (err: unknown) {
    status.value = AsyncStatus.ERROR;
    console.error(err);
  }
}
</script>