<template>
  <LoadingPanel :status="status" :error="errorMessage">
    <form
      :class="{ 'was-validated': submitAttempted }"
      novalidate
      @submit.prevent="handleSubmit"
    >
      <fieldset>
        <legend>Update Cloud File</legend>
        <p>
          Editing
          <strong>{{ file?.filename }}</strong>
          — the filename on the server is preserved regardless of how the
          contents are supplied.
        </p>

        <ul class="nav nav-tabs mb-2">
          <li class="nav-item">
            <button
              type="button"
              class="nav-link"
              :class="{ active: mode === 'inline' }"
              @click="mode = 'inline'"
            >
              Edit contents
            </button>
          </li>
          <li class="nav-item">
            <button
              type="button"
              class="nav-link"
              :class="{ active: mode === 'upload' }"
              @click="mode = 'upload'"
            >
              Upload file
            </button>
          </li>
        </ul>

        <div v-if="mode === 'inline'">
          <div class="form-group">
            <textarea
              v-model="contents"
              class="form-control"
              rows="20"
              spellcheck="false"
              style="font-family: monospace; white-space: pre;"
              :disabled="contentsLoading"
            />
            <small
              v-if="jsonError"
              class="form-text text-danger"
            >Invalid JSON: {{ jsonError }} (you can still save — the server
              does not require JSON, but most MCP files are JSON).</small>
          </div>
          <div class="d-flex gap-2 mb-2">
            <button
              type="button"
              class="btn btn-outline-secondary btn-sm"
              :disabled="contentsLoading || jsonError !== null"
              @click="prettyPrint"
            >
              Pretty-print JSON
            </button>
            <button
              type="button"
              class="btn btn-outline-secondary btn-sm"
              :disabled="contentsLoading"
              @click="loadContents"
            >
              Reload from server
            </button>
          </div>
        </div>

        <div v-else class="form-group row">
          <label for="file" class="col-sm-12 col-form-label">File</label>
          <div class="col-sm-6">
            <input
              id="file"
              type="file"
              class="form-control"
              name="file"
              required
              @change="handleFileChange($event.target)"
            />
            <div class="invalid-feedback">File is required</div>
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
          <button type="submit" class="btn btn-primary">Update File</button>
        </div>
      </fieldset>
    </form>
  </LoadingPanel>
</template>

<script setup lang="ts">
import { computed, onMounted, PropType, shallowRef, ref } from 'vue';
import { AsyncStatus } from '@/types/async-status';
import LoadingPanel from '@/components/LoadingPanel.vue';
import AdminService from '@/services/admin.service';
import { ICloudFile } from '../types/cloud-file';

const props = defineProps({
  file: {
    type: Object as PropType<ICloudFile>,
    required: true
  }
});

const emit = defineEmits(['updated', 'cancel']);

const adminService = new AdminService();

const status = shallowRef(AsyncStatus.OK);
const mode = shallowRef<'inline' | 'upload'>('inline');
const contents = ref<string>('');
const contentsLoading = shallowRef(false);
const updatedFile = shallowRef<File | undefined>(undefined);
const submitAttempted = shallowRef(false);
const errorMessage = shallowRef('Error updating file. Please try again.');

const jsonError = computed<string | null>(() => {
  if (!contents.value.trim()) return null;
  try {
    JSON.parse(contents.value);
    return null;
  } catch (e) {
    return (e as Error).message;
  }
});

async function loadContents() {
  contentsLoading.value = true;
  try {
    contents.value = await adminService.getCloudFileText(props.file.filename);
  } catch (err: unknown) {
    contents.value = '';
    errorMessage.value = `Could not load file contents: ${(err as Error)?.message}`;
    status.value = AsyncStatus.ERROR;
  } finally {
    contentsLoading.value = false;
  }
}

function prettyPrint() {
  try {
    contents.value = JSON.stringify(JSON.parse(contents.value), null, '\t');
  } catch {
    // jsonError computed already surfaces the message; do nothing.
  }
}

function handleFileChange(eventTarget: EventTarget | null) {
  const target = eventTarget as HTMLInputElement;
  if (!target?.files) {
    return;
  }
  updatedFile.value = target.files[0];
}

async function handleSubmit() {
  submitAttempted.value = true;
  const formData = new FormData();

  if (mode.value === 'inline') {
    const blob = new Blob([contents.value], { type: 'application/octet-stream' });
    formData.append('file', blob, props.file.filename);
  } else {
    if (!updatedFile.value) {
      return;
    }
    formData.append('file', updatedFile.value, props.file.filename);
  }

  try {
    status.value = AsyncStatus.BUSY;
    await adminService.upsertCloudFile(formData);
    status.value = AsyncStatus.OK;
    emit('updated');
  } catch (err: unknown) {
    status.value = AsyncStatus.ERROR;
    errorMessage.value = (err as Error)?.message;
  }
}

onMounted(loadContents);
</script>
