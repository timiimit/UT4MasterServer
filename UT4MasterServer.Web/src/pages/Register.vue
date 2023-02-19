<template>
  <LoadingPanel :status="status" :error="errorMessage">
    <form
      :class="{ 'was-validated': submitAttempted }"
      novalidate
      @submit.prevent="register"
    >
      <fieldset>
        <legend>Register</legend>
        <div class="form-group row">
          <label for="username" class="col-sm-12 col-form-label">Email</label>
          <div class="col-sm-6">
            <input
              id="email"
              v-model="email"
              type="email"
              class="form-control"
              name="email"
              required
              placeholder="Email"
              autocomplete="email"
            />
            <div class="invalid-feedback">Email is required</div>
          </div>
        </div>
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
              autocomplete="off"
            />
            <div class="invalid-feedback">Username is required</div>
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
              v-valid="passwordValid"
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
        <div v-if="recaptchaSiteKey.length" class="form-group row">
          <VueRecaptcha
            :sitekey="recaptchaSiteKey"
            :load-recaptcha-script="true"
            @verify="handleRecaptchaSuccess"
            @error="handleRecaptchaError"
          />
          <input v-valid="recaptchaValid" type="text" class="visibly-hidden" />
          <div class="invalid-feedback">Recaptcha validation failed</div>
        </div>
        <div class="form-group row">
          <div class="col-sm-12">
            <button
              type="submit"
              class="btn btn-primary"
              :disabled="status === AsyncStatus.BUSY"
            >
              Register
            </button>
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
import { IRegisterRequest } from '@/types/register-request';

const username = shallowRef('');
const password = shallowRef('');
const email = shallowRef('');
const status = shallowRef(AsyncStatus.OK);
const emailValid = computed(() => validateEmail(email.value));
const passwordValid = computed(() => validatePassword(password.value));
const recaptchaValid = shallowRef(false);
const formValid = computed(
  () =>
    emailValid.value &&
    username.value &&
    passwordValid.value &&
    recaptchaValid.value
);
const errorMessage = shallowRef('Error registering account. Please try again.');
const submitAttempted = shallowRef(false);
const recaptchaSiteKey = __RECAPTCHA_SITE_KEY;
const recaptchaToken = shallowRef<string | undefined>(undefined);

const accountService = new AccountService();

const router = useRouter();

function handleRecaptchaSuccess(token: string) {
  recaptchaToken.value = token;
  recaptchaValid.value = true;
}

function handleRecaptchaError() {
  recaptchaValid.value = false;
}

async function register() {
  submitAttempted.value = true;
  if (!formValid.value) {
    return;
  }
  try {
    status.value = AsyncStatus.BUSY;
    const formData: IRegisterRequest = {
      email: email.value,
      username: username.value,
      password: CryptoJS.SHA512(password.value).toString()
    };
    if (recaptchaToken.value) {
      formData.recaptchaToken = recaptchaToken.value;
    }
    await accountService.register(formData);
    status.value = AsyncStatus.OK;
    router.push('/Login');
  } catch (err: unknown) {
    status.value = AsyncStatus.ERROR;
    errorMessage.value = (err as Error)?.message;
  }
}
</script>
