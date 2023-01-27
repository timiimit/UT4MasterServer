<template>
  <LoadingPanel :status="status" :error="errorMessage">
    <form :class="{ 'was-validated': submitAttempted }" novalidate @submit.prevent="register">
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
              placeholder="Username" autocomplete="off" />
            <div class="invalid-feedback">Username is required</div>
          </div>
        </div>
        <div class="form-group row">
          <label for="password" class="col-sm-12 col-form-label">Password</label>
          <div class="col-sm-6">
            <input type="password" class="form-control" id="password" name="password" minlength="7" v-model="password"
              placeholder="Password" autocomplete="current-password" v-valid="passwordValid" />
            <div class="invalid-feedback">Password must be at least 7 characters</div>
          </div>
        </div>
        <div class="form-group row">
          <VueRecaptcha :sitekey="recaptchaSiteKey" :load-recaptcha-script="true" @verify="handleRecaptchaSuccess"
            @error="handleRecaptchaError" />
          <input type="text" class="visibly-hidden" v-valid="recaptchaValid" />
          <div class="invalid-feedback">Recaptcha validation failed</div>
        </div>
        <div class="form-group row">
          <div class="col-sm-12">
            <button type="submit" class="btn btn-primary" :disabled="status === AsyncStatus.BUSY">Register</button>
          </div>
        </div>
      </fieldset>
    </form>
  </LoadingPanel>
</template>

<script setup lang="ts">
import LoadingPanel from '@/components/LoadingPanel.vue';
import { AsyncStatus } from '@/types/async-status';
import { computed, shallowRef } from 'vue';
import { validateEmail, validatePassword } from '@/utils/validation';
import CryptoJS from 'crypto-js';
import { useRouter } from 'vue-router';
import AccountService from '@/services/account.service';
import { valid as vValid } from '@/directives/valid';
import { VueRecaptcha } from 'vue-recaptcha';

const username = shallowRef('');
const password = shallowRef('');
const email = shallowRef('');
const status = shallowRef(AsyncStatus.OK);
const emailValid = computed(() => validateEmail(email.value));
const passwordValid = computed(() => validatePassword(password.value));
const recaptchaValid = shallowRef(false);
const formValid = computed(() => emailValid.value && username.value && passwordValid.value && recaptchaValid.value);
const errorMessage = shallowRef('Error registering account. Please try again.');
const submitAttempted = shallowRef(false);
const recaptchaSiteKey = __RECAPTCHA_SITE_KEY;
const recaptchaToken = shallowRef<string | undefined>(undefined);

const accountService = new AccountService();

const router = useRouter();

function handleRecaptchaSuccess(token: string) {
  recaptchaToken.value = token
  recaptchaValid.value = true;
}

function handleRecaptchaError() {
  recaptchaValid.value = false;
}

async function register() {
  submitAttempted.value = true;
  if (!formValid.value) { return; }
  try {
    status.value = AsyncStatus.BUSY;
    const formData = {
      email: email.value,
      username: username.value,
      password: CryptoJS.SHA512(password.value).toString(),
      recaptchaToken: recaptchaToken.value
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
