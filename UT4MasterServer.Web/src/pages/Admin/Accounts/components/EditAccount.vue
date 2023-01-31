<template>
    <EditAccountFlags :account="account" @updated="$emit('updated')" />
    <hr />
    <ChangePassword :account="account" @updated="$emit('updated')" />
</template>

<script lang="ts" setup>

import { IAccount } from '@/types/account';
import { PropType } from 'vue';
import ChangePassword from './ChangePassword.vue';
import EditAccountFlags from './EditAccountFlags.vue';

defineProps({
    account: {
        type: Object as PropType<IAccount>,
        required: true
    }
});

defineEmits(['updated']);
</script>