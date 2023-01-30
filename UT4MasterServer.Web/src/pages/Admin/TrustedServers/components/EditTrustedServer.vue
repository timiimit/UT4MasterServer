<template>
    <LoadingPanel :status="status" :error="errorMessage">
        <form :class="{ 'was-validated': submitAttempted }" @submit.prevent="handleSubmit" novalidate>
            <fieldset>
                <legend>Edit Trusted Server</legend>
                <div class="form-group row">
                    <label for="name" class="col-sm-12 col-form-label">Client</label>
                    <div class="col-sm-6">
                        <select class="form-select form-control" v-model="clientId" required disabled readonly>
                            <option :value="option.id" v-for="option in clientOptions">
                                {{ option.name }}
                            </option>
                        </select>
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
                    <button type="submit" class="btn btn-primary">Update Trusted Server</button>
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
import { shallowRef, computed, PropType } from 'vue';
import { AsyncStatus } from '@/types/async-status';
import LoadingPanel from '@/components/LoadingPanel.vue';
import AdminService from '@/services/admin-service';
import { IGridTrustedServer, ITrustedGameServer } from '../types/trusted-game-server';
import { GameServerTrust } from '@/enums/game-server-trust';

const props = defineProps({
    server: {
        type: Object as PropType<IGridTrustedServer>,
        required: true
    }
});

const emit = defineEmits(['updated', 'cancel']);

const adminService = new AdminService();

const status = shallowRef(AsyncStatus.OK);
const clientId = shallowRef(props.server.id);
const trustLevel = shallowRef(props.server.trustLevel);
const submitAttempted = shallowRef(false);
const formValid = computed(() => clientId.value.length);
const errorMessage = shallowRef('Error updating trusted server. Please try again.');

const clientOptions = [props.server.client!];
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
        const trustedServer: ITrustedGameServer = {
            ...props.server,
            trustLevel: trustLevel.value
        };
        await adminService.updateTrustedServer(props.server.id, trustedServer);
        status.value = AsyncStatus.OK;
        emit('updated');
    }
    catch (err: unknown) {
        status.value = AsyncStatus.ERROR;
        errorMessage.value = (err as Error)?.message;
    }
}
</script>
