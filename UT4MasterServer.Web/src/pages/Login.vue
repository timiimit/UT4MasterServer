<template>
  <form>
    <fieldset>
      <legend>Log In</legend>
      <div class="form-group row">
        <label for="username" class="col-sm-12 col-form-label">Username</label>
        <div class="col-sm-6">
          <input type="text" class="form-control" id="username" name="username" required v-model="username" placeholder="Username" autocomplete="username" />
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
          <input type="password" class="form-control" id="password" name="password" minlength="7" v-model="password" placeholder="Password" autocomplete="current-password" />
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
import { computed, shallowRef } from 'vue';
import CryptoJS from 'crypto-js';
import AuthenticationService from '../services/authentication.service';

const username = shallowRef(localStorage.getItem('ut4uu_username') ?? '');
const password = shallowRef('');
const saveUsername = shallowRef(!!localStorage.getItem('ut4uu_username'));
const formValid = computed(() => username.value && password.value.length > 7);

const authenticationService = new AuthenticationService();

function logIn() {
  const formData = {
    username: username.value,
    password: CryptoJS.SHA512(password.value).toString()
  };
  if(saveUsername.value) {
    localStorage.setItem('ut4uu_username', username.value!);
  } else {
    localStorage.removeItem('ut4uu_username');
  }
  console.debug('log in', formData);
  authenticationService.logIn(formData);
}
</script>