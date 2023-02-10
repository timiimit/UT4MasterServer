<template>
  <LoadingPanel :status="status" :error="errorMessage">
    <form
      :class="{ 'was-validated': submitAttempted }"
      novalidate
      @submit.prevent="handleSubmit"
    >
      <fieldset>
        <legend>Change Password</legend>
        <div class="form-group row">
          <label for="newPassword" class="col-sm-12 col-form-label"
            >New Password</label
          >
          <div class="col-sm-6"> 
            <input
              id="newPassword"
              v-model="newPassword"
              type="text"
              class="form-control"
              name="newPassword"
              required
              placeholder="New Password"
              autocomplete="off"
              autofocus
            />
            <div class="invalid-feedback">New Password is required</div>
            <div
              v-if="disableForm"
              class="invalid-feedback"
              :class="{ show: disableForm }"
            >
              You do not have permission to change this account's password
            </div>
          </div>
          <label for="email" class="col-sm-12 col-form-label"
            >Email address</label
          >
          <div class="col-sm-6">
            <input
              id="email"
              v-model="email"
              type="text"
              class="form-control"
              name="email"
              required
              placeholder="user@mail.com"
              autocomplete="off"
              autofocus
            />
            <div class="invalid-feedback">Email address is required</div>
          </div>
          <div class="col-sm-6 flex-v-center">
            <div class="form-check">
              <input
                id="iAmSure"
                v-model="iAmSure"
                class="form-check-input"
                type="checkbox"
                name="iAmSure"
                required
              />
              <label class="form-check-label" for="iAmSure"> I am sure </label>
            </div>
          </div>
        </div>
        <div class="form-group row">
          <div class="col-sm-12">
            <button
              :disabled="disableForm"
              type="submit"
              class="btn btn-primary"
            >
              Change Password
            </button>
          </div>
        </div>
      </fieldset>
    </form>
  </LoadingPanel>
</template>

<script setup lang="ts">
import { shallowRef, PropType, computed } from 'vue';
import { AsyncStatus } from '@/types/async-status';
import LoadingPanel from '@/components/LoadingPanel.vue';
import { IAccountWithRoles } from '@/types/account';
import AdminService from '@/services/admin.service';
import CryptoJS from 'crypto-js';
import { Role } from '@/enums/role';
import { validateEmail, validatePassword } from '@/utils/validation';

const props = defineProps({
  account: {
    type: Object as PropType<IAccountWithRoles>,
    required: true
  }
});

const emit = defineEmits(['updated']);

const adminService = new AdminService();

const status = shallowRef(AsyncStatus.OK);
const newPassword = shallowRef('');
const email = shallowRef('');
const iAmSure = shallowRef(false);
const submitAttempted = shallowRef(false);
const errorMessage = shallowRef(
  'Error changing account password. Please try again.'
);
const passwordValid = computed(() => validatePassword(newPassword.value));
const emailValid = computed(() => validateEmail(email.value));
const formValid = computed(() => passwordValid.value && emailValid.value && iAmSure.value);

// Don't allow changing admin or moderator password
const disableForm = [Role.Admin, Role.Moderator].some((r) =>
  props.account?.roles?.includes(r)
);

async function handleSubmit() {
  submitAttempted.value = true;
  if (!formValid.value || disableForm) {
    return;
  }
  try {
    status.value = AsyncStatus.BUSY;
    const request = {
      newPassword: CryptoJS.SHA512(newPassword.value).toString(),
      email: email.value,
      iAmSure: iAmSure.value
    };
    await adminService.changePassword(props.account.id, request);
    status.value = AsyncStatus.OK;
    emit('updated');
  } catch (err: unknown) {
    status.value = AsyncStatus.ERROR;
    errorMessage.value = (err as Error)?.message;
  }
}
</script>
