<template>
    <LoadingPanel :status="status" :error="errorMessage">
        <form :class="{ 'was-validated': submitAttempted }" @submit.prevent="handleSubmit" novalidate>
            <fieldset>
                <legend>Add Trusted Server</legend>
                <div class="form-group row">
                    <label for="name" class="col-sm-12 col-form-label">Client</label>
                    <div class="col-sm-6">
                        <select class="form-select" v-model="clientId" required>
                            <option :value="option.id" v-for="option in clientOptions">
                                {{ option.name }}
                            </option>
                        </select>
                        <div class="invalid-feedback">Client is required</div>
                    </div>
                </div>
                <div class="form-group row">
                    <label for="name" class="col-sm-12 col-form-label">Trust Level</label>
                    <div class="col-sm-6">
                        <select class="form-select" v-model="trustLevel" required>
                            <option :value="option.value" v-for="option in trustLevelOptions">
                                {{ option.text }}
                            </option>
                        </select>
                        <div class="invalid-feedback">Trust Level is required</div>
                    </div>
                </div>
                <div class="d-flex justify-content-between mb-2">
                    <button type="button" class="btn btn-secondary" @click="emit('cancel')">Cancel</button>
                    <button type="submit" class="btn btn-primary">Add Trusted Server</button>
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
import { useClientOptions } from '../../Clients/hooks/use-client-options.hook';
import { IClient } from '../../Clients/types/client';
import { ITrustedGameServer } from '../types/trusted-game-server';
import { GameServerTrust } from '@/enums/game-server-trust';

const props = defineProps({
    clients: {
        type: Array as PropType<IClient[]>,
        required: true
    },
    servers: {
        type: Array as PropType<ITrustedGameServer[]>,
        required: true
    }
});

const emit = defineEmits(['added', 'cancel']);

const { getClientOptionsForTrustedServer } = useClientOptions();

const adminService = new AdminService();

const status = shallowRef(AsyncStatus.OK);
const clientId = shallowRef('');
const trustLevel = shallowRef(GameServerTrust.Untrusted);
const submitAttempted = shallowRef(false);
const formValid = computed(() => clientId.value.length);
const errorMessage = shallowRef('Error adding trusted server. Please try again.');

const clientOptions = getClientOptionsForTrustedServer(props.clients, props.servers);
const trustLevelOptions = [
    { text: 'Epic', value: GameServerTrust.Epic },
    { text: 'Trusted', value: GameServerTrust.Trusted },
    { text: 'Untrusted', value: GameServerTrust.Untrusted }
];

async function handleSubmit() {
    submitAttempted.value = true;
    if (!formValid.value) { return; }
    try {
        status.value = AsyncStatus.BUSY;
        const trustedServer: Partial<ITrustedGameServer> = {
            id: clientId.value,
            trustLevel: trustLevel.value
        };
        console.debug('create', trustedServer);
        await adminService.createTrustedServer(trustedServer);
        status.value = AsyncStatus.OK;
        emit('added');
    }
    catch (err: unknown) {
        status.value = AsyncStatus.ERROR;
        errorMessage.value = (err as Error)?.message;
    }
}

</script>

