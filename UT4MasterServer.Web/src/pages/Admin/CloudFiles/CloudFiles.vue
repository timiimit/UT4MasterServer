<template>
  <CrudPage title="Cloud Files">
    <template #add="p">
      <AddCloudFile
        :all-files="files"
        @cancel="p.cancel"
        @added="
          loadFiles();
          p.cancel();
        "
      />
    </template>
    <template #filters>
      <div>
        <input
          v-model="filterText"
          type="text"
          class="form-control"
          placeholder="Filter by Name..."
        />
      </div>
    </template>
    <LoadingPanel :status="status" auto-load @load="loadFiles">
      <table class="table">
        <thead>
          <tr>
            <th>Name</th>
            <th>Uploaded</th>
            <th style="width: 8rem">Size</th>
            <th />
          </tr>
        </thead>
        <tbody>
          <template
            v-for="file in filteredFiles.slice(pageStart, pageEnd)"
            :key="objectHash(file)"
          >
            <tr :class="{ 'table-light': file.editing }">
              <td>{{ file.filename }}</td>
              <td>{{ isoDateTimeStringToLocalDateTime(file.uploadedAt) }}</td>
              <td>{{ getFileSize(file.length) }}</td>
              <td class="actions">
                <button
                  class="btn btn-icon"
                  @click="file.editing = !file.editing"
                >
                  <FontAwesomeIcon icon="fa-regular fa-pen-to-square" />
                </button>
                <button
                  v-if="canDelete(file)"
                  class="btn btn-icon"
                  @click="handleDelete(file)"
                >
                  <FontAwesomeIcon icon="fa-solid fa-trash-can" />
                </button>
              </td>
            </tr>
            <tr v-if="file.editing" class="edit-row table-light">
              <td colspan="3">
                <EditFile
                  :file="file"
                  @cancel="file.editing = false"
                  @updated="handleUpdated(file)"
                />
              </td>
            </tr>
          </template>
        </tbody>
      </table>
      <Paging
        :item-count="filteredFiles.length"
        :page-size="pageSize"
        @update="handlePagingUpdate"
      />
    </LoadingPanel>
  </CrudPage>
</template>

<style lang="scss" scoped>
td.actions {
  width: 6rem;

  button:not(:last-child) {
    margin-right: 1rem;
  }
}
</style>

<script lang="ts" setup>
import { shallowRef, ref, computed } from 'vue';
import CrudPage from '@/components/CrudPage.vue';
import LoadingPanel from '@/components/LoadingPanel.vue';
import { AsyncStatus } from '@/types/async-status';
import {
  objectHash,
  isoDateTimeStringToLocalDateTime
} from '@/utils/utilities';
import AdminService from '@/services/admin.service';
import { ICloudFile } from './types/cloud-file';
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';
import AddCloudFile from './components/AddCloudFile.vue';
import EditFile from './components/EditCloudFile.vue';
import Paging from '@/components/Paging.vue';
import { usePaging } from '@/hooks/use-paging.hook';

interface IGridCloudFile extends ICloudFile {
  editing?: boolean;
}

const restrictedFiles = [
  'UnrealTournamentOnlineSettings.json',
  'UnrealTournmentMCPAnnouncement.json',
  'UnrealTournmentMCPGameRulesets.json',
  'UnrealTournmentMCPStorage.json',
  'UTMCPPlaylists.json'
];

const adminService = new AdminService();
const files = ref<IGridCloudFile[]>([]);
const status = shallowRef(AsyncStatus.OK);
const filterText = shallowRef('');
const filteredFiles = computed(() =>
  files.value.filter((c) =>
    c.filename
      .toLocaleLowerCase()
      .includes(filterText.value.toLocaleLowerCase())
  )
);

const { pageSize, pageStart, pageEnd, handlePagingUpdate } = usePaging();

function getFileSize(fileLength: number) {
  if (fileLength < 1024) {
    return `${fileLength} B`;
  }
  return `${Math.round(fileLength / 1024)} KB`;
}

async function loadFiles() {
  try {
    status.value = AsyncStatus.BUSY;
    files.value = await adminService.getCloudFiles();
    console.debug(files.value);
    status.value = AsyncStatus.OK;
  } catch (err: unknown) {
    status.value = AsyncStatus.ERROR;
    console.error('Error loading files', err);
  }
}

function canDelete(file: IGridCloudFile) {
  return !restrictedFiles.includes(file.filename);
}

function handleUpdated(file: IGridCloudFile) {
  file.editing = false;
  loadFiles();
}

async function handleDelete(file: IGridCloudFile) {
  const confirmDelete = confirm(
    `Are you sure you want to delete file ${file.filename}?`
  );
  if (confirmDelete) {
    try {
      status.value = AsyncStatus.BUSY;
      await adminService.deleteCloudFile(file.filename);
      loadFiles();
      status.value = AsyncStatus.OK;
    } catch (err: unknown) {
      status.value = AsyncStatus.ERROR;
      console.error('Error deleting file', err);
    }
  }
}
</script>
