<template>
  <form :class="{ 'was-validated': submitAttempted }" @submit.prevent="changePassword" novalidate>
    <fieldset>
      <legend>Change Username</legend>
      <div class="form-group row">
        <label for="currentUsername" class="col-sm-12 col-form-label">Current Username</label>
        <div class="col-sm-6">
          <input type="text" class="form-control" id="currentUsername" name="currentUsername"
            v-model="currentUsername" readonly disabled />
        </div>
      </div>
      <div class="form-group row">
        <label for="newUsername" class="col-sm-12 col-form-label">New Username</label>
        <div class="col-sm-6">
          <input type="text" class="form-control" id="newUsername" name="newUsername"
            v-model="newUsername" required />
          <div class="invalid-feedback">New Username is required</div>
        </div>
      </div>
      <div class="form-group row">
        <div class="col-sm-12">
          <button type="submit" class="btn btn-primary">Change Username</button>
        </div>
      </div>
    </fieldset>
  </form>
</template>

<script setup lang="ts">
import { IChangeUsernameRequest } from '../types/change-username-request';
import { shallowRef, computed } from 'vue';
import { SessionStore } from '../stores/session-store';

const currentUsername = shallowRef(SessionStore.username);
const newUsername = shallowRef<string>('');
const submitAttempted = shallowRef(false);

const formValid = computed(() => newUsername.value?.length);

function changePassword() {
  submitAttempted.value = true;
  if (!formValid.value) { return; }
  const request: IChangeUsernameRequest = {
    newUsername: newUsername.value
  };
  console.debug('Change username request', request);
  // TODO: call accountService when backend implemented
}
</script>

