<template>
  <form :class="{ 'was-validated': submitAttempted }" @submit.prevent="changePassword" novalidate>
    <fieldset>
      <legend>Change Password</legend>
      <div class="form-group row">
        <label for="currentPassword" class="col-sm-12 col-form-label">Current Password</label>
        <div class="col-sm-6">
          <input type="password" class="form-control" id="currentPassword" name="currentPassword" required
            v-model="currentPassword" placeholder="Current Password" autocomplete="off" minlength="7" />
          <div class="invalid-feedback">Current password must be at least 7 characters</div>
        </div>
      </div>
      <div class="form-group row">
        <label for="newPassword" class="col-sm-12 col-form-label">New Password</label>
        <div class="col-sm-6">
          <input type="password" class="form-control" id="newPassword" name="newPassword" 
            v-model="newPassword" placeholder="New Password" autocomplete="off" required minlength="7" v-valid="newPasswordValid" />
          <div v-if="!newPasswordLength" class="invalid-feedback">New Password must be at least 7 characters</div>
          <div v-if="!newPasswordDiffers" class="invalid-feedback">New Password must differ from Current Password</div>
        </div>
      </div>
      <div class="form-group row">
        <label for="confirmPassword" class="col-sm-12 col-form-label">Confirm Password</label>
        <div class="col-sm-6">
          <input type="password" class="form-control" v-model="confirmPassword"
            placeholder="Confirm Password" autocomplete="off" required minlength="7" v-valid="confirmPasswordValid" />
          <div v-if="!confirmPasswordValid" class="invalid-feedback">Confirm Password must match New Password</div>
        </div>
      </div>
      <div class="form-group row">
        <div class="col-sm-12">
          <button type="submit" class="btn btn-primary">Change Password</button>
        </div>
      </div>
    </fieldset>
  </form>
</template>

<script setup lang="ts">
import { IChangePasswordRequest } from '../types/change-password-request';
import { shallowRef, computed } from 'vue';
import { valid as vValid } from '../directives/valid';
import CryptoJS from 'crypto-js';

const currentPassword = shallowRef<string>('');
const newPassword = shallowRef<string>('');
const confirmPassword = shallowRef<string>('');
const submitAttempted = shallowRef(false);

const currentPasswordValid = computed(() => currentPassword.value.length > 6);
const newPasswordLength = computed(() => newPassword.value.length > 6);
const newPasswordDiffers = computed(() => newPassword.value !== currentPassword.value);
const newPasswordValid = computed(() => newPasswordDiffers.value && newPasswordLength.value);
const confirmPasswordValid = computed(() => confirmPassword.value === newPassword.value);
const formValid = computed(() => currentPasswordValid.value && newPasswordValid.value && confirmPasswordValid.value);

function changePassword() {
  submitAttempted.value = true;
  if (!formValid.value) { return; }
  const request: IChangePasswordRequest = {
    currentPassword: CryptoJS.SHA512(currentPassword.value).toString(),
    newPassword: CryptoJS.SHA512(currentPassword.value).toString()
  };
  console.debug('Change password request', request);
  // TODO: call accountService when backend implemented
}
</script>

