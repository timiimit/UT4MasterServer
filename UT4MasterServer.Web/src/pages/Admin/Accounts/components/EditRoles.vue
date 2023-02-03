<template>
  <LoadingPanel :status="status" :error="errorMessage">
    <form
      :class="{ 'was-validated': submitAttempted }"
      novalidate
      @submit.prevent="handleSubmit"
    >
      <fieldset>
        <legend>Account Roles</legend>
        <div class="form-group row">
          <div class="col-sm-6">
            <RoleMultiSelect
              v-model="roles"
              :exclude-role-options="excludeRoles"
            />
            <div class="invalid-feedback" :class="{ show: disableForm }">
              You do not have permission to change this account's roles
            </div>
          </div>
        </div>
        <div class="form-group row">
          <div class="col-sm-12">
            <button
              type="submit"
              class="btn btn-primary"
              :disabled="disableForm"
            >
              Update Roles
            </button>
          </div>
        </div>
      </fieldset>
    </form>
  </LoadingPanel>
</template>

<style src="@vueform/multiselect/themes/default.css"></style>

<script setup lang="ts">
import { shallowRef, PropType } from 'vue';
import { AsyncStatus } from '@/types/async-status';
import LoadingPanel from '@/components/LoadingPanel.vue';
import { IAccountWithRoles } from '@/types/account';
import AdminService from '@/services/admin-service';
import RoleMultiSelect from './RoleMultiSelect.vue';
import { Role } from '@/enums/role';
import { AccountStore } from '@/stores/account-store';

const props = defineProps({
  account: {
    type: Object as PropType<IAccountWithRoles>,
    required: true
  }
});

const emit = defineEmits(['updated']);

const adminService = new AdminService();

const status = shallowRef(AsyncStatus.OK);
const roles = shallowRef<Role[]>(props.account.roles);
const submitAttempted = shallowRef(false);
const errorMessage = shallowRef(
  'Error updating account roles. Please try again.'
);

// Don't allow moderator to give admin role to anyone
const userIsAdmin = AccountStore.account?.roles?.includes(Role.Admin);
const excludeRoles = userIsAdmin ? [] : [Role.Admin];
// Don't allow moderator to edit admin's roles at all
const accountIsAdmin = props.account?.roles?.includes(Role.Admin);
const disableForm = accountIsAdmin && !userIsAdmin;

async function handleSubmit() {
  submitAttempted.value = true;
  if (disableForm) {
    return;
  }
  try {
    status.value = AsyncStatus.BUSY;
    await adminService.setRolesForAccount(props.account.id, roles.value);
    status.value = AsyncStatus.OK;
    emit('updated');
  } catch (err: unknown) {
    status.value = AsyncStatus.ERROR;
    errorMessage.value = (err as Error)?.message;
  }
}
</script>
