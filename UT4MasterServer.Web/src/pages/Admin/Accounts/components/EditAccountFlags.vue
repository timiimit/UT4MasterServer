<template>
  <LoadingPanel :status="status" :error="errorMessage">
    <form :class="{ 'was-validated': submitAttempted }" @submit.prevent="handleSubmit" novalidate>
      <fieldset>
        <legend>Account Flags</legend>
        <div class="form-group row">
          <div class="col-sm-6">
            <Multiselect v-model="flags" :options="flagOptions" mode="tags" />
          </div>
        </div>
        <div class="form-group row">
          <div class="col-sm-12">
            <button type="submit" class="btn btn-primary">Update Flags</button>
          </div>
        </div>
      </fieldset>
    </form>

  </LoadingPanel>
</template>

<style src="@vueform/multiselect/themes/default.css">

</style>

<script setup lang="ts">
import { shallowRef, computed, onMounted, PropType } from 'vue';
import { AsyncStatus } from '@/types/async-status';
import LoadingPanel from '@/components/LoadingPanel.vue';
import { IAccount } from '@/types/account';
import AdminService from '@/services/admin-service';
import Multiselect from '@vueform/multiselect';

const props = defineProps({
  account: {
    type: Object as PropType<IAccount>,
    required: true
  }
})

const adminService = new AdminService();

const status = shallowRef(AsyncStatus.OK);
const flags = shallowRef<string[]>([]);
const allFlags = shallowRef<string[]>([]);
const submitAttempted = shallowRef(false);
const errorMessage = shallowRef('Error updating account flags. Please try again.');

const flagOptions = computed(() => allFlags.value.map((f) => ({ label: f, value: f })));

async function handleSubmit() {
  submitAttempted.value = true;
  try {
    status.value = AsyncStatus.BUSY;
    await adminService.setFlagsForAccount(props.account.id, flags.value);
    status.value = AsyncStatus.OK;
  }
  catch (err: unknown) {
    status.value = AsyncStatus.ERROR;
    errorMessage.value = (err as Error)?.message;
  }
}

async function loadData() {
  try {
    status.value = AsyncStatus.BUSY;
    const [allPossibleFlags, accountFlags] = await Promise.all([adminService.getAccountFlagOptions(), adminService.getFlagsForAccount(props.account.id)]);
    flags.value = accountFlags;
    allFlags.value = allPossibleFlags;
    status.value = AsyncStatus.OK;
  }
  catch (err: unknown) {
    console.error('Error loading account flag data', err);
    status.value = AsyncStatus.ERROR;
  }
}

onMounted(loadData);
</script>

