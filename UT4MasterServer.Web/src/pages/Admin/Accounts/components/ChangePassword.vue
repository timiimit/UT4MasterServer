<template>
  <LoadingPanel :status="status" :error="errorMessage">
    <form :class="{ 'was-validated': submitAttempted }" @submit.prevent="handleSubmit" novalidate>
      <fieldset>
        <legend>Change Password</legend>
        <div class="form-group row">
          <label for="newPassword" class="col-sm-12 col-form-label">New Password</label>
          <div class="col-sm-6">
            <input type="text" class="form-control" id="newPassword" name="newPassword" required v-model="newPassword"
              placeholder="New Password" autocomplete="off" autofocus />
            <div class="invalid-feedback">New Password is required</div>
          </div>
          <div class="col-sm-6 flex-v-center">
            <div class="form-check">
              <input class="form-check-input" type="checkbox" id="iAmSure" name="iAmSure" v-model="iAmSure" required />
              <label class="form-check-label" for="iAmSure">
                I am sure
              </label>
            </div>
          </div>
        </div>
        <div class="form-group row">
          <div class="col-sm-12">
            <button type="submit" class="btn btn-primary">Change Password</button>
          </div>
        </div>
      </fieldset>
    </form>

  </LoadingPanel>
</template>

<script setup lang="ts">
import { shallowRef, PropType } from 'vue';
import { AsyncStatus } from '@/types/async-status';
import LoadingPanel from '@/components/LoadingPanel.vue';
import { IAccount } from '@/types/account';
import AdminService from '@/services/admin-service';
import CryptoJS from 'crypto-js';

const props = defineProps({
  account: {
    type: Object as PropType<IAccount>,
    required: true
  }
})

const emit = defineEmits(['updated']);

const adminService = new AdminService();

const status = shallowRef(AsyncStatus.OK);
const newPassword = shallowRef('');
const iAmSure = shallowRef(false);
const submitAttempted = shallowRef(false);
const errorMessage = shallowRef('Error changing account password. Please try again.');

async function handleSubmit() {
  submitAttempted.value = true;
  try {
    status.value = AsyncStatus.BUSY;
    const request = {
      newPassword: CryptoJS.SHA512(newPassword.value).toString(),
      iAmSure: iAmSure.value
    };
    await adminService.changePassword(props.account.ID, request);
    status.value = AsyncStatus.OK;
    emit('updated');
  }
  catch (err: unknown) {
    status.value = AsyncStatus.ERROR;
    errorMessage.value = (err as Error)?.message;
  }
}
</script>
