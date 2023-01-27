<template>
    <h1>Trusted Servers</h1>
</template>

<script lang="ts" setup>
import { onMounted, shallowRef } from 'vue';
import { ITrustedGameServer } from '@/types/trusted-game-server';
import AdminService from '@/services/admin-service';

const adminService = new AdminService();
const trustedServers = shallowRef<ITrustedGameServer[]>([]);

onMounted(async () => {
    trustedServers.value = await adminService.getTrustedServers();
    console.debug('trusted', trustedServers.value);
});
</script>