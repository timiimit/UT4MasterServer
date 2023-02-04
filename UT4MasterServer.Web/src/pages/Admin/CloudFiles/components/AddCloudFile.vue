<template>
  <LoadingPanel :status="status" :error="errorMessage">
    <form
      :class="{ 'was-validated': submitAttempted }"
      novalidate
      @submit.prevent="handleSubmit"
    >
      <fieldset>
        <legend>Add Cloud File</legend>
        <div class="form-group row">
          <label for="name" class="col-sm-12 col-form-label">Name</label>
          <div class="col-sm-6">
            <input
              id="name"
              v-valid="fileValid"
              type="file"
              class="form-control"
              name="name"
              required
              @change="handleFileChange($event.target)"
            />
            <div class="invalid-feedback">A unique filename is required</div>
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
          <button type="submit" class="btn btn-primary">Add File</button>
        </div>
      </fieldset>
    </form>
  </LoadingPanel>
</template>

<script setup lang="ts">
import { shallowRef, PropType, computed } from 'vue';
import { AsyncStatus } from '@/types/async-status';
import LoadingPanel from '@/components/LoadingPanel.vue';
import AdminService from '@/services/admin.service';
import { ICloudFile } from '../types/cloud-file';
import { valid as vValid } from '@/directives/valid';

const props = defineProps({
  allFiles: {
    type: Array as PropType<ICloudFile[]>,
    default: () => []
  }
});

const emit = defineEmits(['added', 'cancel']);

const adminService = new AdminService();

const status = shallowRef(AsyncStatus.OK);
const name = shallowRef('');
const content = shallowRef<string | ArrayBuffer | null>(null);
const fileValid = computed(
  () => !props.allFiles.map((f) => f.filename).includes(name.value)
);
const submitAttempted = shallowRef(false);
const errorMessage = shallowRef('Error adding client. Please try again.');

function handleFileChange(eventTarget: EventTarget | null) {
  const target = eventTarget as HTMLInputElement;
  if (!target?.files) {
    return;
  }
  let reader = new FileReader();
  if (target.files.length > 0) {
    let file = target.files[0];
    reader.readAsDataURL(file);
    reader.onload = () => {
      content.value = reader.result;
    };
  }
}

async function handleSubmit() {
  submitAttempted.value = true;
  try {
    status.value = AsyncStatus.BUSY;
    await adminService.createCloudFile(name.value, content.value);
    status.value = AsyncStatus.OK;
    emit('added');
  } catch (err: unknown) {
    status.value = AsyncStatus.ERROR;
    errorMessage.value = (err as Error)?.message;
  }
}
</script>
