<template>
  <LoadingPanel :status="status" :error="errorMessage">
    <form
      :class="{ 'was-validated': submitAttempted }"
      novalidate
      @submit.prevent="handleSubmit"
    >
      <fieldset>
        <legend>Edit Trusted Server</legend>
        <div class="form-group row">
          <label for="name" class="col-sm-12 col-form-label">Client</label>
          <div class="col-sm-6">
            <select
              v-model="clientId"
              class="form-select form-control"
              required
              disabled
              readonly
            >
              <option
                v-for="option in clientOptions"
                :key="option.id"
                :value="option.id"
              >
                {{ option.name }}
              </option>
            </select>
          </div>
        </div>
        <div class="form-group row">
          <label for="name" class="col-sm-12 col-form-label">Owner</label>
          <div class="col-sm-6">
            <select
              v-model="ownerId"
              class="form-select"
              name="ownerId"
              required
            >
              <option
                v-for="option in ownerOptions"
                :key="option.value"
                :value="option.value"
              >
                {{ option.text }}
              </option>
            </select>
            <div class="invalid-feedback">Owner is required</div>
          </div>
        </div>
        <div class="form-group row">
          <label for="name" class="col-sm-12 col-form-label">Trust Level</label>
          <div class="col-sm-6">
            <select v-model="trustLevel" class="form-select" required>
              <option
                v-for="option in trustLevelOptions"
                :key="option.value"
                :value="option.value"
              >
                {{ option.text }}
              </option>
            </select>
            <div class="invalid-feedback">Trust Level is required</div>
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
          <button type="submit" class="btn btn-primary">
            Update Trusted Server
          </button>
        </div>
      </fieldset>
    </form>
  </LoadingPanel>
</template>

<style lang="scss" scoped>
.form-select:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
</style>

<script setup lang="ts">
import { shallowRef, computed, PropType, onMounted } from 'vue';
import { AsyncStatus } from '@/types/async-status';
import LoadingPanel from '@/components/LoadingPanel.vue';
import AdminService from '@/services/admin.service';
import {
  IGridTrustedServer,
  ITrustedGameServer
} from '../types/trusted-game-server';
import { GameServerTrust } from '@/enums/game-server-trust';
import { Role } from '@/enums/role';
import { IAccount } from '@/types/account';
import AccountService from '@/services/account.service';

const props = defineProps({
  server: {
    type: Object as PropType<IGridTrustedServer>,
    required: true
  }
});

const emit = defineEmits(['updated', 'cancel']);

const adminService = new AdminService();
const accountService = new AccountService();

const status = shallowRef(AsyncStatus.OK);
const clientId = shallowRef(props.server.id);
const ownerId = shallowRef(props.server.ownerID);
const trustLevel = shallowRef(props.server.trustLevel);
const submitAttempted = shallowRef(false);
const formValid = computed(() => clientId.value.length);
const errorMessage = shallowRef(
  'Error updating trusted server. Please try again.'
);

const clientOptions = [props.server.client];
const owners = shallowRef<IAccount[]>([]);
const ownerOptions = computed(() => {
  return owners.value.map((a) => ({ text: a.username, value: a.id }));
});
const trustLevelOptions = [
  { text: 'Epic', value: GameServerTrust.Epic },
  { text: 'Trusted', value: GameServerTrust.Trusted },
  { text: 'Untrusted', value: GameServerTrust.Untrusted }
];

async function handleSubmit() {
  submitAttempted.value = true;
  if (!formValid.value) {
    return;
  }
  try {
    status.value = AsyncStatus.BUSY;
    const trustedServer: ITrustedGameServer = {
      ...props.server,
      ownerID: ownerId.value,
      trustLevel: trustLevel.value
    };
    await adminService.updateTrustedServer(props.server.id, trustedServer);
    status.value = AsyncStatus.OK;
    emit('updated');
  } catch (err: unknown) {
    status.value = AsyncStatus.ERROR;
    errorMessage.value = (err as Error)?.message;
  }
}

async function loadAccounts() {
  try {
    status.value = AsyncStatus.BUSY;
    const response = await accountService.searchAccounts('', 0, 1000, false, [
      Role.HubOwner
    ]);
    owners.value = response.accounts;
    status.value = AsyncStatus.OK;
  } catch (err: unknown) {
    status.value = AsyncStatus.ERROR;
    errorMessage.value = 'Error loading accounts.';
  }
}

onMounted(loadAccounts);
</script>
