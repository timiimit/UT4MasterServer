<template>
    <EditAccountFlags :account="account" />
</template>

<script lang="ts" setup>

import { IAccount } from '@/types/account';
import { PropType } from 'vue';
import EditAccountFlags from './EditAccountFlags.vue';


defineProps({
    account: {
        type: Object as PropType<IAccount>,
        required: true
    }
})
</script>