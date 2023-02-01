<template>
  <LoadingPanel :status="status" :error="errorMessage">
    <form
      :class="{ 'was-validated': submitAttempted }"
      novalidate
      @submit.prevent="handleSubmit"
    >
      <fieldset>
        <legend>Edit Client</legend>
        <div class="form-group row">
          <label for="name" class="col-sm-12 col-form-label">Name</label>
          <div class="col-sm-6">
            <input
              id="name"
              v-model="name"
              type="text"
              class="form-control"
              name="name"
              required
            />
            <div class="invalid-feedback">Name is required</div>
          </div>
        </div>
        <div class="form-group row">
          <label for="name" class="col-sm-12 col-form-label">Secret</label>
          <div class="col-sm-6">
            <input
              id="secret"
              v-model="secret"
              type="text"
              class="form-control"
              name="secret"
              required
              minlength="20"
            />
            <div class="invalid-feedback">
              Secret must be at least 20 characters
            </div>
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
          <button type="submit" class="btn btn-primary">Update Client</button>
        </div>
      </fieldset>
    </form>
  </LoadingPanel>
</template>

<script lang="ts" setup>
import { PropType } from 'vue';
import { IClient } from '../types/client';
import { shallowRef, computed } from 'vue';
import { AsyncStatus } from '@/types/async-status';
import LoadingPanel from '@/components/LoadingPanel.vue';
import AdminService from '@/services/admin-service';

const props = defineProps({
  client: {
    type: Object as PropType<IClient>,
    required: true
  }
});

const emit = defineEmits(['updated', 'cancel']);

const adminService = new AdminService();

const status = shallowRef(AsyncStatus.OK);
const name = shallowRef(props.client.name);
const secret = shallowRef(props.client.secret);
const submitAttempted = shallowRef(false);
const formValid = computed(
  () => name.value.length && secret.value.length >= 20
);
const errorMessage = shallowRef('Error updating client. Please try again.');

async function handleSubmit() {
  submitAttempted.value = true;
  if (!formValid.value) {
    return;
  }
  try {
    status.value = AsyncStatus.BUSY;
    const updatedClient = {
      ...props.client,
      name: name.value,
      secret: secret.value
    };
    await adminService.updateClient(props.client.id, updatedClient);
    status.value = AsyncStatus.OK;
    emit('updated');
  } catch (err: unknown) {
    status.value = AsyncStatus.ERROR;
    errorMessage.value = (err as Error)?.message;
  }
}
</script>
