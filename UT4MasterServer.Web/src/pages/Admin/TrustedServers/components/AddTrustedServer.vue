<template>
  <LoadingPanel :status="status" :error="errorMessage">
    <form
      :class="{ 'was-validated': submitAttempted }"
      novalidate
      @submit.prevent="handleSubmit"
    >
      <fieldset>
        <legend>Add Trusted Server</legend>
        <div class="form-group row">
          <label for="name" class="col-sm-12 col-form-label">Client</label>
          <div class="col-sm-6">
            <select
              v-model="clientId"
              class="form-select"
              name="clientId"
              required
            >
              <option
                v-for="option in clientOptions"
                :key="option.id"
                :value="option.id"
              >
                {{ option.name }}
              </option>
            </select>
            <div class="invalid-feedback">Client is required</div>
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
            Add Trusted Server
          </button>
        </div>
      </fieldset>
    </form>
  </LoadingPanel>
</template>

<script setup lang="ts">
import { shallowRef, computed, PropType, onMounted } from 'vue';
import { AsyncStatus } from '@/types/async-status';
import LoadingPanel from '@/components/LoadingPanel.vue';
import AdminService from '@/services/admin-service';
import { useClientOptions } from '../../Clients/hooks/use-client-options.hook';
import { IClient } from '../../Clients/types/client';
import { ITrustedGameServer } from '../types/trusted-game-server';
import { GameServerTrust } from '@/enums/game-server-trust';
import { AccountStore } from '@/stores/account-store';
import { AccountFlag } from '@/enums/account-flag';

const props = defineProps({
  clients: {
    type: Array as PropType<IClient[]>,
    required: true,
  },
  servers: {
    type: Array as PropType<ITrustedGameServer[]>,
    required: true,
  },
});

const emit = defineEmits(['added', 'cancel']);

const { getClientOptionsForTrustedServer } = useClientOptions();

const adminService = new AdminService();

const status = shallowRef(AsyncStatus.OK);
const clientId = shallowRef('');
const ownerId = shallowRef('');
const trustLevel = shallowRef(GameServerTrust.Untrusted);
const submitAttempted = shallowRef(false);
const formValid = computed(() => clientId.value.length && ownerId.value.length);
const errorMessage = shallowRef(
  'Error adding trusted server. Please try again.'
);

const clientOptions = getClientOptionsForTrustedServer(
  props.clients,
  props.servers
);
const ownerOptions = computed(() => {
  const hubOwners = AccountStore.accounts?.filter((a) =>
    a.Roles?.includes(AccountFlag.HubOwner)
  );
  return hubOwners?.map((a) => ({ text: a.Username, value: a.ID }));
});
const trustLevelOptions = [
  { text: 'Epic', value: GameServerTrust.Epic },
  { text: 'Trusted', value: GameServerTrust.Trusted },
  { text: 'Untrusted', value: GameServerTrust.Untrusted },
];

async function handleSubmit() {
  submitAttempted.value = true;
  if (!formValid.value) {
    return;
  }
  try {
    status.value = AsyncStatus.BUSY;
    const trustedServer: Partial<ITrustedGameServer> = {
      id: clientId.value,
      ownerID: ownerId.value,
      trustLevel: trustLevel.value,
    };
    console.debug('create', trustedServer);
    await adminService.createTrustedServer(trustedServer);
    status.value = AsyncStatus.OK;
    emit('added');
  } catch (err: unknown) {
    status.value = AsyncStatus.ERROR;
    errorMessage.value = (err as Error)?.message;
  }
}

async function loadAccounts() {
  if (AccountStore.accounts?.length) {
    return;
  }
  try {
    status.value = AsyncStatus.BUSY;
    await AccountStore.fetchAllAccounts();
    status.value = AsyncStatus.OK;
  } catch (err: unknown) {
    status.value = AsyncStatus.ERROR;
    errorMessage.value = 'Error loading accounts.';
  }
}

onMounted(loadAccounts);
</script>
