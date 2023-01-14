<template>
  <LoadingPanel :status="status" :error="errorMessage">
    <form @submit.prevent="register">
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
            <button type="submit" class="btn btn-primary" :disabled="!formValid">Register</button>
          </div>
        </div>
      </fieldset>
    </form>
  </LoadingPanel>
</template>

<script setup lang="ts">
import LoadingPanel from '../components/LoadingPanel.vue';
import { AsyncStatus } from '../types/async-status';
import { computed, shallowRef } from 'vue';
import { validateEmail } from '../utils/validation';
import CryptoJS from 'crypto-js';
import { useRouter } from 'vue-router';
import AccountService from '../services/account.service';

const username = shallowRef('');
const password = shallowRef('');
const email = shallowRef('');
const status = shallowRef(AsyncStatus.OK);
const formValid = computed(() => validateEmail(email.value) && username.value && password.value.length > 7 && status.value != AsyncStatus.BUSY);
const errorMessage = shallowRef('Error registering account. Please try again.');

const accountService = new AccountService();

const router = useRouter();

async function register() {
  try {
    status.value = AsyncStatus.BUSY;
    const formData = {
      email: email.value,
      username: username.value,
      password: CryptoJS.SHA512(password.value).toString(),
    };
    await accountService.register(formData);
    status.value = AsyncStatus.OK;
    router.push('/Login');
  } catch (err: unknown) {
    status.value = AsyncStatus.ERROR;
    errorMessage.value = (err as Error)?.message;
  }
}
</script>