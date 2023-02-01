<template>
  <LoadingPanel :status="status" :error="errorMessage">
    <form
      :class="{ 'was-validated': submitAttempted }"
      novalidate
      @submit.prevent="handleSubmit"
    >
      <fieldset>
        <legend>Add Client</legend>
        <div class="form-group row">
          <label for="name" class="col-sm-12 col-form-label">Name</label>
          <div class="col-sm-6">
            <input
              id="name"
              v-model="name"
              v-valid="formValid"
              type="text"
              class="form-control"
              name="name"
              required
            />
            <div class="invalid-feedback">A unique name is required</div>
          </div>
        </div>
        <div class="d-flex justify-content-between mb-2">
          <button
            type="button"
            class="btn btn-secondary"
            @click="emit('cancel')"
          >
            Cancel
          </button>
          <button type="submit" class="btn btn-primary">Add Client</button>
        </div>
      </fieldset>
    </form>
  </LoadingPanel>
</template>

<script setup lang="ts">
import { shallowRef, computed, PropType } from 'vue';
import { AsyncStatus } from '@/types/async-status';
import LoadingPanel from '@/components/LoadingPanel.vue';
import AdminService from '@/services/admin-service';
import { IClient } from '../types/client';
import { useClientOptions } from '../hooks/use-client-options.hook';
import { valid as vValid } from '@/directives/valid';

const props = defineProps({
  allClients: {
    type: Array as PropType<IClient[]>,
    required: true
  }
});

const emit = defineEmits(['added', 'cancel']);

const { isValidName } = useClientOptions();

const adminService = new AdminService();

const status = shallowRef(AsyncStatus.OK);
const name = shallowRef('');
const submitAttempted = shallowRef(false);
const formValid = computed(() => isValidName(name.value, props.allClients));
const errorMessage = shallowRef('Error adding client. Please try again.');

async function handleSubmit() {
  submitAttempted.value = true;
  if (!formValid.value) {
    return;
  }
  try {
    status.value = AsyncStatus.BUSY;
    await adminService.createClient(name.value);
    status.value = AsyncStatus.OK;
    emit('added');
  } catch (err: unknown) {
    status.value = AsyncStatus.ERROR;
    errorMessage.value = (err as Error)?.message;
  }
}
</script>
